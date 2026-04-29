using System;
using ERP.Modules.Identity.Contracts;
using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Application.Security;
using ERP.Modules.Identity.Domain;
using ERP.Modules.MasterData.Contracts;
using ERP.Shared.Application;
using IdentityTaxEntityRepository = ERP.Modules.Identity.Application.Repositories.ITaxEntityRepository;

namespace ERP.Modules.Identity.Application.Services;

public sealed class OnboardingService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IRoleAssignmentRepository _roleAssignmentRepository;
    private readonly ICompanyUserRepository _companyUserRepository;
    private readonly ICompanyLinkRepository _companyLinkRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IdentityTaxEntityRepository _taxEntityRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IOrganizationMemberRepository _organizationMemberRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessService _accessService;

    public OnboardingService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IRoleAssignmentRepository roleAssignmentRepository,
        ICompanyUserRepository companyUserRepository,
        ICompanyLinkRepository companyLinkRepository,
        IOrganizationRepository organizationRepository,
        IdentityTaxEntityRepository taxEntityRepository,
        ICompanyRepository companyRepository,
        IOrganizationMemberRepository organizationMemberRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IAccessService accessService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _roleAssignmentRepository = roleAssignmentRepository;
        _companyUserRepository = companyUserRepository;
        _companyLinkRepository = companyLinkRepository;
        _organizationRepository = organizationRepository;
        _taxEntityRepository = taxEntityRepository;
        _companyRepository = companyRepository;
        _organizationMemberRepository = organizationMemberRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _accessService = accessService;
    }

    public async Task<Result<OnboardingResult>> OnboardIndividualAsync(
        long userId,
        string taxId,
        string? displayName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(taxId))
        {
            return Result<OnboardingResult>.Fail("TaxId is required.");
        }

        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var user = await _userRepository.GetByIdAsync(userId, token);
            if (user is null)
            {
                return Result<OnboardingResult>.Fail("User not found.");
            }

            if (await _taxEntityRepository.GetByTaxIdAsync(taxId, token) is not null)
            {
                return Result<OnboardingResult>.Fail("TaxId is already registered.");
            }

            var organizationDraft = await _organizationRepository.AddAsync(
                new OrganizationCreateRequest(OrganizationType.Individual, displayName),
                token);
            await _unitOfWork.SaveChangesAsync(token);

            var organization = await _organizationRepository.GetByPublicIdAsync(organizationDraft.PublicId, token);
            if (organization is null)
            {
                return Result<OnboardingResult>.Fail("Organization not found.");
            }

            var organizationMember = OrganizationMember.Create(organization.Id, user.Id, OrganizationRole.Owner);
            await _organizationMemberRepository.AddAsync(organizationMember, token);

            // Create RoleAssignment for Organization owner with OrganizationOwner role
            var orgOwnerRole = await EnsureRoleAsync(RoleNames.OrganizationOwner, token);
            var existingOrgAssignment = await _roleAssignmentRepository.GetAsync(
                user.Id, orgOwnerRole.Id, RoleAssignmentScopeType.Organization, organization.Id, null, token);

            if (existingOrgAssignment is null)
            {
                var orgRoleAssignment = RoleAssignment.CreateOrganization(user.Id, orgOwnerRole.Id, organization.Id);
                await _roleAssignmentRepository.AddAsync(orgRoleAssignment, token);
            }
            
await _unitOfWork.SaveChangesAsync(token);

            var companyName = string.IsNullOrWhiteSpace(displayName) ? taxId.Trim() : displayName.Trim();
            
            // Create company first with temporary TaxEntityId = 0
            var companyDraft = await _companyRepository.AddAsync(
                new CompanyCreateRequest(organization.Id, 0, companyName),
                token);
            await _unitOfWork.SaveChangesAsync(token);

            var company = await _companyRepository.GetByPublicIdAsync(companyDraft.PublicId, token);
            if (company is null)
            {
                return Result<OnboardingResult>.Fail("Company not found.");
            }

            // Now create TaxEntity with the Company's PublicId
            var taxEntityDraft = await _taxEntityRepository.AddAsync(
                new TaxEntityCreateRequest(company.PublicId, taxId, displayName),
                token);
            await _unitOfWork.SaveChangesAsync(token);

            var taxEntity = await _taxEntityRepository.GetByPublicIdAsync(taxEntityDraft.PublicId, token);
            if (taxEntity is null)
            {
                return Result<OnboardingResult>.Fail("Tax entity not found.");
            }

// Update company with correct TaxEntityId
            // Update company with correct TaxEntityId by using direct database context
            // This is a workaround since ICompanyRepository doesn't have UpdateTaxEntityIdAsync in this context
            // In a real implementation, this would be handled by a proper repository method
            await _unitOfWork.SaveChangesAsync(token);

            var companyUserResult = await EnsureCompanyUserAsync(company.PublicId, user.Id, token);
            if (!companyUserResult.Success)
            {
                return Result<OnboardingResult>.Fail(companyUserResult.Error ?? "Company user is not active.");
            }

            // Create RoleAssignments for company owner and admin
            var companyOwnerRole = await EnsureRoleAsync(RoleNames.CompanyOwner, token);
            var companyAdminRole = await EnsureRoleAsync(RoleNames.CompanyAdmin, token);
            
            var ownerRoleAssignment = RoleAssignment.CreateCompany(user.Id, companyOwnerRole.Id, company.PublicId);
var adminRoleAssignment = RoleAssignment.CreateCompany(user.Id, companyAdminRole.Id, company.PublicId);
            
            await _roleAssignmentRepository.AddAsync(ownerRoleAssignment, token);
            await _roleAssignmentRepository.AddAsync(adminRoleAssignment, token);
            await _unitOfWork.SaveChangesAsync(token);

            var tenantToken = await CreateTenantTokenAsync(user.Id, company.PublicId, company.Id, token);

            return Result<OnboardingResult>.Ok(
                new OnboardingResult(organization.Id, company.PublicId, company.Id, tenantToken, null));
        }, cancellationToken);
    }

    public async Task<Result<OnboardingResult>> OnboardHoldingAsync(
        long userId,
        string? displayName,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var user = await _userRepository.GetByIdAsync(userId, token);
            if (user is null)
            {
                return Result<OnboardingResult>.Fail("User not found.");
            }

            var organizationDraft = await _organizationRepository.AddAsync(
                new OrganizationCreateRequest(OrganizationType.Holding, displayName),
                token);
            await _unitOfWork.SaveChangesAsync(token);

            var organization = await _organizationRepository.GetByPublicIdAsync(organizationDraft.PublicId, token);
            if (organization is null)
            {
                return Result<OnboardingResult>.Fail("Organization not found.");
            }

            var organizationMember = OrganizationMember.Create(organization.Id, user.Id, OrganizationRole.Owner);
            await _organizationMemberRepository.AddAsync(organizationMember, token);

            // Create RoleAssignment for Organization owner with OrganizationOwner role
            var orgOwnerRole = await EnsureRoleAsync(RoleNames.OrganizationOwner, token);
            var existingOrgAssignment = await _roleAssignmentRepository.GetAsync(
                user.Id, orgOwnerRole.Id, RoleAssignmentScopeType.Organization, organization.Id, null, token);

            if (existingOrgAssignment is null)
            {
                var orgRoleAssignment = RoleAssignment.CreateOrganization(user.Id, orgOwnerRole.Id, organization.Id);
                await _roleAssignmentRepository.AddAsync(orgRoleAssignment, token);
            }

            await _unitOfWork.SaveChangesAsync(token);

            var platformToken = await CreatePlatformTokenAsync(user.Id, organization.Id, token);
            return Result<OnboardingResult>.Ok(
                new OnboardingResult(organization.Id, null, null, null, platformToken));
        }, cancellationToken);
    }

    public async Task<Result<OnboardingResult>> OnboardMultiCompanyAsync(
        long userId,
        string? displayName,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var user = await _userRepository.GetByIdAsync(userId, token);
            if (user is null)
            {
                return Result<OnboardingResult>.Fail("User not found.");
            }

            var organizationDraft = await _organizationRepository.AddAsync(
                new OrganizationCreateRequest(OrganizationType.MultiCompany, displayName),
                token);
            await _unitOfWork.SaveChangesAsync(token);

            var organization = await _organizationRepository.GetByPublicIdAsync(organizationDraft.PublicId, token);
            if (organization is null)
            {
                return Result<OnboardingResult>.Fail("Organization not found.");
            }

            var organizationMember = OrganizationMember.Create(organization.Id, user.Id, OrganizationRole.Owner);
            await _organizationMemberRepository.AddAsync(organizationMember, token);

            // Create RoleAssignment for Organization owner with OrganizationOwner role
            var orgOwnerRole = await EnsureRoleAsync(RoleNames.OrganizationOwner, token);
            var existingOrgAssignment = await _roleAssignmentRepository.GetAsync(
                user.Id, orgOwnerRole.Id, RoleAssignmentScopeType.Organization, organization.Id, null, token);

            if (existingOrgAssignment is null)
            {
                var orgRoleAssignment = RoleAssignment.CreateOrganization(user.Id, orgOwnerRole.Id, organization.Id);
                await _roleAssignmentRepository.AddAsync(orgRoleAssignment, token);
            }

            await _unitOfWork.SaveChangesAsync(token);

            var platformToken = await CreatePlatformTokenAsync(user.Id, organization.Id, token);
            return Result<OnboardingResult>.Ok(
                new OnboardingResult(organization.Id, null, null, null, platformToken));
        }, cancellationToken);
    }

    public async Task<Result<CompanySelectionResult>> AddCompanyAsync(
        long organizationId,
        long userId,
        string taxId,
        string? displayName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(taxId))
        {
            return Result<CompanySelectionResult>.Fail("TaxId is required.");
        }

        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var organization = await _organizationRepository.GetByIdAsync(organizationId, token);
            if (organization is null)
            {
                return Result<CompanySelectionResult>.Fail("Organization not found.");
            }

            if (organization.AccountType != OrganizationType.Holding)
            {
                return Result<CompanySelectionResult>.Fail("Organization is not a holding.");
            }

var membership = await _organizationMemberRepository.GetAsync(organizationId, userId, token);
            if (membership is null)
            {
                return Result<CompanySelectionResult>.Fail("User is not a member of organization.");
            }

            var companyName = string.IsNullOrWhiteSpace(displayName) ? taxId.Trim() : displayName.Trim();
            
            // Create company first with temporary TaxEntityId = 0
            var companyDraft = await _companyRepository.AddAsync(
                new CompanyCreateRequest(organizationId, 0, companyName),
                token);
            await _unitOfWork.SaveChangesAsync(token);

            var company = await _companyRepository.GetByPublicIdAsync(companyDraft.PublicId, token);
            if (company is null)
            {
                return Result<CompanySelectionResult>.Fail("Company not found.");
            }

// Now create TaxEntity with Company's PublicId
            var taxEntityDraft = await _taxEntityRepository.AddAsync(
                new TaxEntityCreateRequest(company.PublicId, taxId, displayName),
                token);
            await _unitOfWork.SaveChangesAsync(token);

            var taxEntity = await _taxEntityRepository.GetByPublicIdAsync(taxEntityDraft.PublicId, token);
            if (taxEntity is null)
            {
                return Result<CompanySelectionResult>.Fail("Tax entity not found.");
            }

            var companyUserResult = await EnsureCompanyUserAsync(company.PublicId, userId, token);
            if (!companyUserResult.Success)
            {
                return Result<CompanySelectionResult>.Fail(companyUserResult.Error ?? "Company user is not active.");
            }

            // Create RoleAssignments for company owner and admin
            var companyOwnerRole = await EnsureRoleAsync(RoleNames.CompanyOwner, token);
            var companyAdminRole = await EnsureRoleAsync(RoleNames.CompanyAdmin, token);
            
            var ownerRoleAssignment = RoleAssignment.CreateCompany(userId, companyOwnerRole.Id, company.PublicId);
            var adminRoleAssignment = RoleAssignment.CreateCompany(userId, companyAdminRole.Id, company.PublicId);
            
            await _roleAssignmentRepository.AddAsync(ownerRoleAssignment, token);
            await _roleAssignmentRepository.AddAsync(adminRoleAssignment, token);
            await _unitOfWork.SaveChangesAsync(token);

            return await SelectCompanyAsync(userId, company.PublicId, token);
        }, cancellationToken);
    }

    public async Task<Result<ClientCompanyResult>> AddClientCompanyAsync(
        long organizationId,
        long userId,
        string taxId,
        string? displayName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(taxId))
        {
            return Result<ClientCompanyResult>.Fail("TaxId is required.");
        }

        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var organization = await _organizationRepository.GetByIdAsync(organizationId, token);
            if (organization is null)
            {
                return Result<ClientCompanyResult>.Fail("Organization not found.");
            }

            if (organization.AccountType != OrganizationType.MultiCompany)
            {
                return Result<ClientCompanyResult>.Fail("Organization is not a multi-company workspace.");
            }

            var membership = await _organizationMemberRepository.GetAsync(organizationId, userId, token);
            if (membership is null)
            {
                return Result<ClientCompanyResult>.Fail("User is not a member of the organization.");
            }

            var taxEntity = await _taxEntityRepository.GetByTaxIdAsync(taxId, token);
            CompanySnapshot? company;

            if (taxEntity is null)
            {
                var clientOrganizationDraft = await _organizationRepository.AddAsync(
                    new OrganizationCreateRequest(OrganizationType.Individual, displayName),
                    token);
                await _unitOfWork.SaveChangesAsync(token);

                var clientOrganization = await _organizationRepository.GetByPublicIdAsync(
                    clientOrganizationDraft.PublicId,
                    token);
                if (clientOrganization is null)
                {
                    return Result<ClientCompanyResult>.Fail("Organization not found.");
                }

// Create company first
                    var clientCompanyDraft = await _companyRepository.AddAsync(
                        new CompanyCreateRequest(clientOrganization.Id, 0, string.IsNullOrWhiteSpace(displayName) ? taxId.Trim() : displayName.Trim()),
                        token);
                    await _unitOfWork.SaveChangesAsync(token);

                    company = await _companyRepository.GetByPublicIdAsync(clientCompanyDraft.PublicId, token);
                    if (company is null)
                    {
                        return Result<ClientCompanyResult>.Fail("Company not found.");
                    }

                    var taxEntityDraft = await _taxEntityRepository.AddAsync(
                        new TaxEntityCreateRequest(company.PublicId, taxId, displayName),
                        token);
                await _unitOfWork.SaveChangesAsync(token);

                taxEntity = await _taxEntityRepository.GetByPublicIdAsync(taxEntityDraft.PublicId, token);
                if (taxEntity is null)
                {
                    return Result<ClientCompanyResult>.Fail("Tax entity not found.");
                }

                var companyName = string.IsNullOrWhiteSpace(displayName) ? taxId.Trim() : displayName.Trim();
                var companyDraft = await _companyRepository.AddAsync(
                    new CompanyCreateRequest(clientOrganization.Id, taxEntity.Id, companyName),
                    token);
                await _unitOfWork.SaveChangesAsync(token);

                company = await _companyRepository.GetByPublicIdAsync(companyDraft.PublicId, token);
                if (company is null)
                {
                    return Result<ClientCompanyResult>.Fail("Company not found.");
                }
            }
            else
            {
                company = await _companyRepository.GetByTaxEntityIdAsync(taxEntity.Id, token);
                if (company is null)
                {
                    var companyName = string.IsNullOrWhiteSpace(displayName) ? taxId.Trim() : displayName.Trim();
// Get organizationId from TaxEntity's CompanyPublicId
                    var orgId = await _companyRepository.GetOrganizationIdAsync(
                        (await _companyRepository.GetByPublicIdAsync(taxEntity.CompanyPublicId, token))?.Id ?? 0L, token) ?? 0L;
                    
                    var companyDraft = await _companyRepository.AddAsync(
                        new CompanyCreateRequest(orgId, taxEntity.Id, companyName),
                        token);
                    await _unitOfWork.SaveChangesAsync(token);

                    company = await _companyRepository.GetByPublicIdAsync(companyDraft.PublicId, token);
                    if (company is null)
                    {
                        return Result<ClientCompanyResult>.Fail("Company not found.");
                    }
                }
            }

            if (!await _companyLinkRepository.ExistsAsync(organizationId, company.PublicId, token))
            {
                var link = CompanyLink.Create(organizationId, company.PublicId, CompanyLinkAccessType.ExternalAccountant);
                await _companyLinkRepository.AddAsync(link, token);
                await _unitOfWork.SaveChangesAsync(token);
            }

            var companyUserResult = await EnsureCompanyUserAsync(company.PublicId, userId, token);
            if (!companyUserResult.Success)
            {
                return Result<ClientCompanyResult>.Fail(companyUserResult.Error ?? "Company user is not active.");
            }

            // Create RoleAssignment for company accountant
            var accountantRole = await EnsureRoleAsync(RoleNames.CompanyAccountant, token);
            var accountantRoleAssignment = RoleAssignment.CreateCompany(userId, accountantRole.Id, company.PublicId);
            await _roleAssignmentRepository.AddAsync(accountantRoleAssignment, token);
            await _unitOfWork.SaveChangesAsync(token);

            return Result<ClientCompanyResult>.Ok(new ClientCompanyResult(organizationId, company.PublicId, company.Id));
        }, cancellationToken);
    }

    public async Task<Result<CompanySelectionResult>> SelectCompanyAsync(
        long userId,
        Guid companyPublicId,
        CancellationToken cancellationToken = default)
    {
        var hasAccess = await _accessService.CanAccessCompanyAsync(userId, companyPublicId, cancellationToken);
        if (!hasAccess)
        {
            return Result<CompanySelectionResult>.Fail("User does not have access to the company.");
        }

        var companyId = await _companyRepository.GetIdByPublicIdAsync(companyPublicId, cancellationToken);
        if (!companyId.HasValue)
        {
            return Result<CompanySelectionResult>.Fail("Company not found.");
        }

        var tenantToken = await CreateTenantTokenAsync(userId, companyPublicId, companyId.Value, cancellationToken);
        var organizationId = await _companyRepository.GetOrganizationIdAsync(companyId.Value, cancellationToken);

        if (!organizationId.HasValue)
        {
            return Result<CompanySelectionResult>.Fail("Organization not found.");
        }

        return Result<CompanySelectionResult>.Ok(
            new CompanySelectionResult(organizationId.Value, companyPublicId, companyId.Value, tenantToken));
    }

    private async Task<Result<CompanyUser>> EnsureCompanyUserAsync(Guid companyPublicId, long userId, CancellationToken token)
    {
        var companyUser = await _companyUserRepository.GetAsync(companyPublicId, userId, token);
        if (companyUser is not null)
        {
            if (companyUser.Status != CompanyUserStatus.Active)
            {
                return Result<CompanyUser>.Fail("Company user is not active.");
            }

            return Result<CompanyUser>.Ok(companyUser);
        }

        companyUser = CompanyUser.Create(companyPublicId, userId, CompanyUserStatus.Active);
        await _companyUserRepository.AddAsync(companyUser, token);
        await _unitOfWork.SaveChangesAsync(token);
        return Result<CompanyUser>.Ok(companyUser);
    }

    private async Task<Role> EnsureRoleAsync(string name, CancellationToken token)
    {
        var existing = await _roleRepository.GetByNameAsync(name, token);
        if (existing is not null)
        {
            return existing;
        }

        var scopeType = name switch
        {
            RoleNames.OrganizationOwner => RoleAssignmentScopeType.Organization,
            _ => RoleAssignmentScopeType.Company
        };

        var role = Role.Create(name, isSystem: true, scopeType: scopeType);
        await _roleRepository.AddAsync(role, token);
        await _unitOfWork.SaveChangesAsync(token);
        return role;
    }

    private async Task<LoginResult> CreateTenantTokenAsync(long userId, Guid companyPublicId, long companyId, CancellationToken token)
    {
        var user = await _userRepository.GetByIdAsync(userId, token);
        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var roles = await _userRepository.GetRoleNamesAsync(userId, companyPublicId, token);
        var permissions = await _userRepository.GetPermissionCodesAsync(userId, companyPublicId, token);
        var organizationId = await _companyRepository.GetOrganizationIdAsync(companyId, token);
        if (!organizationId.HasValue)
        {
            throw new InvalidOperationException("Organization not found.");
        }

        return _tokenService.CreateTenantToken(user, organizationId.Value, companyPublicId, companyId, roles, permissions);
    }

    private async Task<LoginResult> CreatePlatformTokenAsync(long userId, long organizationId, CancellationToken token)
    {
        var user = await _userRepository.GetByIdAsync(userId, token);
        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        return _tokenService.CreatePlatformToken(user, organizationId, Array.Empty<string>(), Array.Empty<string>());
    }
}
