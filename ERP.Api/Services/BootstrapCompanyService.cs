using ERP.Modules.Identity.Contracts;
using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Domain;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using ERP.Persistence;
using ERP.Persistence.Repositories;
using ERP.Shared.Application;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Services;

public sealed class BootstrapCompanyDefinition
{
    public string? Name { get; set; }
    public string? TaxId { get; set; }
    public string? OrganizationName { get; set; }
}

public sealed class BootstrapOrganizationDefinition
{
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    public OrganizationType? AccountType { get; set; }
}

public sealed record BootstrapCompanySetup(long WorkspaceOrganizationId, IReadOnlyList<long> CompanyIds);

public sealed class BootstrapCompanyService
{
    private readonly ErpDbContext _dbContext;
    private readonly IRoleRepository _roleRepository;
    private readonly IRoleAssignmentRepository _roleAssignmentRepository;

    public BootstrapCompanyService(
        ErpDbContext dbContext,
        IRoleRepository roleRepository,
        IRoleAssignmentRepository roleAssignmentRepository)
    {
        _dbContext = dbContext;
        _roleRepository = roleRepository;
        _roleAssignmentRepository = roleAssignmentRepository;
    }

    public async Task<Result<BootstrapCompanySetup>> EnsureCompaniesAsync(
        BootstrapOrganizationDefinition organizationDefinition,
        IReadOnlyList<BootstrapCompanyDefinition> companies,
        CancellationToken cancellationToken = default)
    {
        if (organizationDefinition.AccountType is null)
        {
            return Result<BootstrapCompanySetup>.Fail("Bootstrap organization configuration is missing.");
        }

        if (companies.Count == 0)
        {
            return Result<BootstrapCompanySetup>.Fail("Bootstrap company configuration is missing.");
        }

        var workspaceDisplayName = ResolveWorkspaceDisplayName(organizationDefinition);
        var workspaceOrganization = await EnsureWorkspaceOrganizationAsync(
            organizationDefinition.AccountType.Value,
            workspaceDisplayName,
            cancellationToken);
        var companyIds = new List<long>();

        foreach (var company in companies)
        {
            if (string.IsNullOrWhiteSpace(company.Name) || string.IsNullOrWhiteSpace(company.TaxId))
            {
                return Result<BootstrapCompanySetup>.Fail("Bootstrap company configuration is missing.");
            }

            var ensuredCompany = organizationDefinition.AccountType.Value == OrganizationType.MultiCompany
                ? await EnsureClientCompanyAsync(company, cancellationToken)
                : await EnsureCompanyAsync(workspaceOrganization.Id, company, cancellationToken);
            await EnsureCompanyLinkAsync(workspaceOrganization.Id, ensuredCompany.PublicId, cancellationToken);
            companyIds.Add(ensuredCompany.Id);
        }

        return Result<BootstrapCompanySetup>.Ok(new BootstrapCompanySetup(workspaceOrganization.Id, companyIds));
    }

    public async Task EnsureWorkspaceOwnerAsync(long organizationId, long userId, CancellationToken cancellationToken = default)
    {
        var member = await _dbContext.OrganizationMembers
            .FirstOrDefaultAsync(entity => entity.OrganizationId == organizationId && entity.UserId == userId, cancellationToken);

        if (member is null)
        {
            await _dbContext.OrganizationMembers.AddAsync(
                OrganizationMember.Create(organizationId, userId, OrganizationRole.Owner),
                cancellationToken);
        }
        else if (member.Role != OrganizationRole.Owner)
        {
            member.UpdateRole(OrganizationRole.Owner);
        }

        // Create RoleAssignment for Organization owner with OrganizationOwner role
        var orgOwnerRole = await _roleRepository.GetByNameAsync(RoleNames.OrganizationOwner, cancellationToken);
        if (orgOwnerRole is null)
        {
            orgOwnerRole = await EnsureRoleAsync(
                RoleNames.OrganizationOwner,
                RoleAssignmentScopeType.Organization,
                cancellationToken);
        }

        var existingOrgAssignment = await _roleAssignmentRepository.GetAsync(
            userId, orgOwnerRole.Id, RoleAssignmentScopeType.Organization, organizationId, null, cancellationToken);

        if (existingOrgAssignment is null)
        {
            var orgRoleAssignment = RoleAssignment.CreateOrganization(userId, orgOwnerRole.Id, organizationId);
            await _roleAssignmentRepository.AddAsync(orgRoleAssignment, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task EnsurePlatformAdminAsync(long userId, CancellationToken cancellationToken = default)
    {
        // Ensure PlatformSuperAdmin role exists (using as platform admin role)
        var platformAdminRole = await _roleRepository.GetByNameAsync(RoleNames.PlatformSuperAdmin, cancellationToken);
        if (platformAdminRole is null)
        {
            platformAdminRole = await EnsureRoleAsync(
                RoleNames.PlatformSuperAdmin,
                RoleAssignmentScopeType.Platform,
                cancellationToken);
        }

        // Check if user already has PlatformAdmin RoleAssignment
        var existingAssignment = await _roleAssignmentRepository.GetAsync(
            userId, platformAdminRole.Id, RoleAssignmentScopeType.Platform, null, null, cancellationToken);

        if (existingAssignment is null)
        {
            var roleAssignment = RoleAssignment.CreatePlatform(userId, platformAdminRole.Id);
            await _roleAssignmentRepository.AddAsync(roleAssignment, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task EnsureCompanyOwnerAsync(long companyId, long userId, CancellationToken cancellationToken = default)
        => await EnsureCompanyRoleAsync(companyId, userId, RoleNames.CompanyOwner, cancellationToken);

    public async Task EnsureRfidManagerAsync(long companyId, long userId, CancellationToken cancellationToken = default)
        => await EnsureCompanyRoleAsync(companyId, userId, RoleNames.RfidManager, cancellationToken);

    private async Task EnsureCompanyRoleAsync(
        long companyId,
        long userId,
        string roleName,
        CancellationToken cancellationToken = default)
    {
        var companyPublicId = await _dbContext.Companies
            .AsNoTracking()
            .Where(entity => entity.Id == companyId)
            .Select(entity => (Guid?)entity.PublicId)
            .SingleOrDefaultAsync(cancellationToken);

        if (!companyPublicId.HasValue)
        {
            return;
        }

        var companyRole = await _roleRepository.GetByNameAsync(roleName, cancellationToken);
        if (companyRole is null)
        {
            companyRole = await EnsureRoleAsync(
                roleName,
                RoleAssignmentScopeType.Company,
                cancellationToken);
        }

        var existingAssignment = await _roleAssignmentRepository.GetAsync(
            userId,
            companyRole.Id,
            RoleAssignmentScopeType.Company,
            null,
            companyPublicId.Value,
            cancellationToken);

        if (existingAssignment is not null)
        {
            return;
        }

        var roleAssignment = RoleAssignment.CreateCompany(userId, companyRole.Id, companyPublicId.Value);
        await _roleAssignmentRepository.AddAsync(roleAssignment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Role> EnsureRoleAsync(
        string name,
        RoleAssignmentScopeType scopeType,
        CancellationToken cancellationToken)
    {
        var existing = await _roleRepository.GetByNameAsync(name, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var role = Role.Create(name, isSystem: true, scopeType: scopeType);
        await _roleRepository.AddAsync(role, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return role;
    }

    private async Task<Organization> EnsureWorkspaceOrganizationAsync(
        OrganizationType accountType,
        string? displayName,
        CancellationToken cancellationToken)
    {
        var normalizedName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
        Organization? organization = null;

        if (!string.IsNullOrWhiteSpace(normalizedName))
        {
            organization = await _dbContext.Organizations
                .FirstOrDefaultAsync(entity => entity.AccountType == accountType
                    && entity.DisplayName == normalizedName, cancellationToken);
        }

        organization ??= await _dbContext.Organizations
            .FirstOrDefaultAsync(entity => entity.AccountType == accountType, cancellationToken);

        if (organization is not null)
        {
            if (!string.IsNullOrWhiteSpace(normalizedName)
                && !string.Equals(organization.DisplayName, normalizedName, StringComparison.Ordinal))
            {
                organization.UpdateDisplayName(normalizedName);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return organization;
        }

        organization = Organization.Create(accountType, normalizedName);
        await _dbContext.Organizations.AddAsync(organization, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return organization;
    }

    private async Task<Company> EnsureCompanyAsync(
        long organizationId,
        BootstrapCompanyDefinition info,
        CancellationToken cancellationToken)
    {
        var companyName = info.Name!.Trim();
        var taxId = info.TaxId!.Trim();
        var taxEntityName = string.IsNullOrWhiteSpace(info.OrganizationName)
            ? companyName
            : info.OrganizationName.Trim();

// Create company first to get the PublicId
        var company = Company.Create(organizationId, 0, companyName); // Temporary TaxEntityId = 0
        
        await _dbContext.Companies.AddAsync(company, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var taxEntity = await _dbContext.TaxEntities
            .FirstOrDefaultAsync(entity => entity.TaxId == taxId && entity.CompanyPublicId == company.PublicId, cancellationToken);

        if (taxEntity is null)
        {
            taxEntity = TaxEntity.Create(company.PublicId, taxId, taxEntityName);
            await _dbContext.TaxEntities.AddAsync(taxEntity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else if (!string.Equals(taxEntity.DisplayName, taxEntityName, StringComparison.Ordinal))
        {
            taxEntity.UpdateDisplayName(taxEntityName);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        // Update company with the correct TaxEntityId
        company.UpdateTaxEntityId(taxEntity.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return company;
    }

    private async Task<Company> EnsureClientCompanyAsync(
        BootstrapCompanyDefinition info,
        CancellationToken cancellationToken)
    {
        var companyName = info.Name!.Trim();
        var organizationDisplayName = string.IsNullOrWhiteSpace(info.OrganizationName)
            ? companyName
            : info.OrganizationName.Trim();
        var taxId = info.TaxId!.Trim();

        var taxEntity = await _dbContext.TaxEntities
            .FirstOrDefaultAsync(entity => entity.TaxId == taxId, cancellationToken);
        Company? company = null;

        if (taxEntity is null)
        {
            var clientOrganization = Organization.Create(OrganizationType.Individual, organizationDisplayName);
            await _dbContext.Organizations.AddAsync(clientOrganization, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Create company first to get PublicId
            company = Company.Create(clientOrganization.Id, 0, companyName);
            await _dbContext.Companies.AddAsync(company, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            taxEntity = TaxEntity.Create(company.PublicId, taxId, organizationDisplayName);
            await _dbContext.TaxEntities.AddAsync(taxEntity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Update company with correct TaxEntityId
            company.UpdateTaxEntityId(taxEntity.Id);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            if (!string.Equals(taxEntity.DisplayName, organizationDisplayName, StringComparison.Ordinal))
            {
                taxEntity.UpdateDisplayName(organizationDisplayName);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            company = await _dbContext.Companies
                .FirstOrDefaultAsync(entity => entity.TaxEntityId == taxEntity.Id, cancellationToken);

            if (company is null)
            {
                // Get organizationId from the TaxEntity's CompanyPublicId
                var orgId = await _dbContext.Companies
                    .Where(c => c.PublicId == taxEntity.CompanyPublicId)
                    .Select(c => c.OrganizationId)
                    .FirstOrDefaultAsync(cancellationToken);
                    
                company = Company.Create(orgId, taxEntity.Id, companyName);
                await _dbContext.Companies.AddAsync(company, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        return company ?? throw new InvalidOperationException("Company could not be resolved.");
    }

    private async Task EnsureCompanyLinkAsync(long organizationId, Guid companyPublicId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.CompanyLinks
            .AsNoTracking()
            .AnyAsync(link => link.OrganizationId == organizationId && link.CompanyPublicId == companyPublicId, cancellationToken);

        if (exists)
        {
            return;
        }

        await _dbContext.CompanyLinks.AddAsync(
            CompanyLink.Create(organizationId, companyPublicId, CompanyLinkAccessType.ExternalAccountant),
            cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string? ResolveWorkspaceDisplayName(BootstrapOrganizationDefinition organization)
    {
        if (!string.IsNullOrWhiteSpace(organization.DisplayName))
        {
            return organization.DisplayName;
        }

        return string.IsNullOrWhiteSpace(organization.Name) ? null : organization.Name;
    }
}
