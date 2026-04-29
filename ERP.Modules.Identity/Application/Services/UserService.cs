using System;
using ERP.Modules.Identity.Contracts;
using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Application.Security;
using ERP.Modules.Identity.Domain;
using ERP.Modules.MasterData.Contracts;
using ERP.Shared.Application;

namespace ERP.Modules.Identity.Application.Services;

public sealed class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly ICompanyUserRepository _companyUserRepository;
    private readonly IOrganizationMemberRepository _organizationMemberRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IRoleAssignmentRepository _roleAssignmentRepository;
    private readonly ICompanyLinkRepository _companyLinkRepository;
    private readonly IWorkspaceResolver _workspaceResolver;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICompanyLookup _companyLookup;
    private readonly ICompanyRepository _companyRepository;

    public UserService(
        IUserRepository userRepository,
        ICompanyUserRepository companyUserRepository,
        IOrganizationMemberRepository organizationMemberRepository,
        IRoleRepository roleRepository,
        IRoleAssignmentRepository roleAssignmentRepository,
        ICompanyLinkRepository companyLinkRepository,
        IWorkspaceResolver workspaceResolver,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ICompanyLookup companyLookup,
        ICompanyRepository companyRepository)
    {
        _userRepository = userRepository;
        _companyUserRepository = companyUserRepository;
        _organizationMemberRepository = organizationMemberRepository;
        _roleRepository = roleRepository;
        _roleAssignmentRepository = roleAssignmentRepository;
        _companyLinkRepository = companyLinkRepository;
        _workspaceResolver = workspaceResolver;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _companyLookup = companyLookup;
        _companyRepository = companyRepository;
    }

    public Task<Result<UserSummary>> CreateUserAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var companyId = _tenantContext.CompanyId ?? 0;
        var companyPublicId = _tenantContext.CompanyPublicId;
        if (companyId <= 0 || !companyPublicId.HasValue)
        {
            return Task.FromResult(Result<UserSummary>.Fail("Company not found."));
        }

        return _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            if (!await _companyLookup.ExistsAsync(companyId, token))
            {
                return Result<UserSummary>.Fail("Company not found.");
            }

            var existingUser = await _userRepository.GetByEmailAsync(email, token);
            User user;

            if (existingUser is not null)
            {
                var existingCompanyUser = await _companyUserRepository.GetAsync(companyPublicId.Value, existingUser.Id, token);
                if (existingCompanyUser is not null)
                {
                    return Result<UserSummary>.Fail("User already exists.");
                }

                user = existingUser;
            }
            else
            {
                var (hash, salt) = _passwordHasher.HashPassword(password);
                user = User.Create(email, hash, salt);
                await _userRepository.AddAsync(user, token);
                await _unitOfWork.SaveChangesAsync(token);
            }

            var companyUser = await _companyUserRepository.GetAsync(companyPublicId.Value, user.Id, token);
            if (companyUser is null)
            {
                companyUser = CompanyUser.Create(companyPublicId.Value, user.Id);
                await _companyUserRepository.AddAsync(companyUser, token);
            }

            return Result<UserSummary>.Ok(new UserSummary(user.Id, companyPublicId.Value, companyId, user.Email, user.IsActive));
        }, cancellationToken);
    }

    public Task<IReadOnlyList<UserWithProfileSummary>> GetUsersAsync(CancellationToken cancellationToken = default)
        => _userRepository.GetAllAsync(cancellationToken);

    public async Task<Result<UserSummary>> UpdateUserAsync(
        Guid companyPublicId,
        long companyId,
        long userId,
        string? email,
        string? password,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<UserSummary>.Fail("User not found.");
        }

        var companyUser = await _companyUserRepository.GetAsync(companyPublicId, userId, cancellationToken);
        if (companyUser is null)
        {
            return Result<UserSummary>.Fail("User not found.");
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            if (!string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _userRepository.GetByEmailAsync(companyPublicId, normalizedEmail, cancellationToken);
                if (existing is not null && existing.Id != user.Id)
                {
                    return Result<UserSummary>.Fail("Email already exists.");
                }

                user.UpdateEmail(normalizedEmail);
            }
        }

        if (!string.IsNullOrWhiteSpace(password))
        {
            var (hash, salt) = _passwordHasher.HashPassword(password);
            user.UpdatePassword(hash, salt);
        }

        if (isActive.HasValue)
        {
            user.SetActive(isActive.Value);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UserSummary>.Ok(new UserSummary(user.Id, companyPublicId, companyId, user.Email, user.IsActive));
    }

    public async Task<Result> DeactivateUserAsync(Guid companyPublicId, long userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result.Fail("User not found.");
        }

        var companyUser = await _companyUserRepository.GetAsync(companyPublicId, userId, cancellationToken);
        if (companyUser is null)
        {
            return Result.Fail("User not found.");
        }

        user.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public Task<Result<UserSummary>> AssociateUserToCompanyAsync(
        Guid userPublicId,
        Guid companyPublicId,
        CancellationToken cancellationToken = default)
    {
        if (userPublicId == Guid.Empty || companyPublicId == Guid.Empty)
        {
            return Task.FromResult(Result<UserSummary>.Fail("Invalid association request."));
        }

        return _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var user = await _userRepository.GetByPublicIdAsync(userPublicId, token);
            if (user is null)
            {
                return Result<UserSummary>.Fail("User not found.");
            }

            var userId = user.Id;

            var company = await _companyRepository.GetByPublicIdAsync(companyPublicId, token);
            if (company is null)
            {
                return Result<UserSummary>.Fail("Company not found.");
            }

            var organizationMember = await _organizationMemberRepository.GetAsync(company.OrganizationId, userId, token);
            if (organizationMember is null)
            {
                await _organizationMemberRepository.AddAsync(
                    OrganizationMember.Create(company.OrganizationId, userId, OrganizationRole.Member),
                    token);

                await _unitOfWork.SaveChangesAsync(token);
            }

            var companyUser = await _companyUserRepository.GetAsync(companyPublicId, userId, token);
            if (companyUser is null)
            {
                companyUser = CompanyUser.Create(companyPublicId, userId, CompanyUserStatus.Active);
                await _companyUserRepository.AddAsync(companyUser, token);
            }
            else if (companyUser.Status != CompanyUserStatus.Active)
            {
                companyUser.SetStatus(CompanyUserStatus.Active);
            }

            var workspaceOrganizationId = await _workspaceResolver.ResolveWorkspaceOrganizationIdAsync(userId, token);
            if (!await _companyLinkRepository.ExistsAsync(workspaceOrganizationId, companyPublicId, token))
            {
                await _companyLinkRepository.AddAsync(
                    CompanyLink.Create(workspaceOrganizationId, companyPublicId, CompanyLinkAccessType.ExternalAccountant),
                    token);
            }

            var companyOwnerRole = await EnsureCompanyOwnerRoleAsync(token);
            var ownerAssignment = await _roleAssignmentRepository.GetAsync(
                userId,
                companyOwnerRole.Id,
                RoleAssignmentScopeType.Company,
                null,
                companyPublicId,
                token);

            if (ownerAssignment is null)
            {
                await _roleAssignmentRepository.AddAsync(
                    RoleAssignment.CreateCompany(userId, companyOwnerRole.Id, companyPublicId),
                    token);
            }
            else if (ownerAssignment.Status != RoleAssignmentStatus.Active)
            {
                ownerAssignment.Activate();
            }

            await _unitOfWork.SaveChangesAsync(token);

            return Result<UserSummary>.Ok(new UserSummary(
                user.Id,
                company.PublicId,
                company.Id,
                user.Email,
                user.IsActive));
        }, cancellationToken);
    }

    private async Task<Role> EnsureCompanyOwnerRoleAsync(CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByNameAsync(RoleAssignmentScopeType.Company, RoleNames.CompanyOwner, cancellationToken);
        if (role is not null)
        {
            return role;
        }

        role = Role.Create(RoleNames.CompanyOwner, isSystem: true, scopeType: RoleAssignmentScopeType.Company);
        await _roleRepository.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return role;
    }
}
