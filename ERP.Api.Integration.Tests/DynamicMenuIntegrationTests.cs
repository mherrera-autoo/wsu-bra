using System.Net.Http.Json;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using ERP.Api.Contracts.Identity;
using ERP.Api.Contracts.Companies;
using ERP.Modules.Identity.Domain;
using ERP.Modules.Identity.Contracts;
using ERP.Modules.MasterData.Domain;
using ERP.Modules.MasterData.Contracts;
using ERP.Persistence;
using ERP.Shared.Domain.ValueObjects;
using ERP.Shared.Application;
using Xunit;

namespace ERP.Api.Integration.Tests;

public sealed class DynamicMenuIntegrationTests
{
    [Fact]
    public async Task GetMyPermissions_UserWithPlatformPermissionsAndNoCompanyMembership_ReturnsEmptyCompanyAndNoMembership()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long userId;
        long organizationId;
        long companyId;
        Guid companyPublicId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var seed = await SeedPlatformUserAsync(dbContext);
            userId = seed.UserId;
            organizationId = seed.OrganizationId;
            companyId = seed.CompanyId;
            companyPublicId = seed.CompanyPublicId;
        }

        using var client = CreateTenantClient(factory, userId, organizationId, companyId, companyPublicId);

        var response = await client.GetFromJsonAsync<UserPermissionsResponse>("/api/identity/me/permissions");

        Assert.NotNull(response);
        Assert.NotNull(response.Platform);
        Assert.Contains(PermissionKeys.Admin.UsersRead, response.Platform);
        Assert.Empty(response.Organization);
        Assert.Empty(response.Company);
        Assert.Equal(companyPublicId, response.Context.CompanyPublicId);
        Assert.Null(response.Context.OrganizationId);
        Assert.False(response.Context.HasCompanyMembership);
    }

    [Fact]
    public async Task GetMyCompanyFeatures_WithoutCompanyUsers_ReturnsForbidden()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long userId;
        long organizationId;
        Guid companyPublicId;

        long companyId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var seed = await SeedCompanyContextWithoutMembershipAsync(dbContext);
            userId = seed.UserId;
            organizationId = seed.OrganizationId;
            companyPublicId = seed.CompanyPublicId;
            companyId = seed.CompanyId;
        }

        using var client = CreateTenantClient(factory, userId, organizationId, companyId, companyPublicId);

        var response = await client.GetAsync("/api/companies/my/features");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task<PlatformSeedContext> SeedPlatformUserAsync(ErpDbContext dbContext)
    {
        var organization = Organization.Create(OrganizationType.Individual, "Platform Org");
        await dbContext.Organizations.AddAsync(organization);
        await dbContext.SaveChangesAsync();

        var company = Company.Create(organization.Id, 0, "Platform Company");
        await dbContext.Companies.AddAsync(company);
        await dbContext.SaveChangesAsync();

        var taxEntity = TaxEntity.Create(company.PublicId, "PLATFORM", "Platform Tax Entity");
        await dbContext.TaxEntities.AddAsync(taxEntity);
        await dbContext.SaveChangesAsync();

        company.UpdateTaxEntityId(taxEntity.Id);
        await dbContext.SaveChangesAsync();

        var user = User.Create("platform@local", "hash", "salt");
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

var permission = Permission.Create(PermissionKeys.Admin.UsersRead, "Admin Users Read");
        var role = Role.Create("Platform Admin", scopeType: RoleAssignmentScopeType.Platform);
        await dbContext.Permissions.AddAsync(permission);
        await dbContext.Roles.AddAsync(role);
        await dbContext.SaveChangesAsync();

        await dbContext.RolePermissions.AddAsync(RolePermission.Create(role.Id, permission.Id));
        await dbContext.SaveChangesAsync();

        await dbContext.RoleAssignments.AddAsync(RoleAssignment.CreatePlatform(user.Id, role.Id));
        await dbContext.SaveChangesAsync();

        return new PlatformSeedContext(user.Id, organization.Id, company.Id, company.PublicId);
    }

    private static async Task<CompanySeedContext> SeedCompanyContextWithoutMembershipAsync(ErpDbContext dbContext)
    {
        var organization = Organization.Create(OrganizationType.Individual, "Company Org");
        await dbContext.Organizations.AddAsync(organization);
        await dbContext.SaveChangesAsync();

        var company = Company.Create(organization.Id, 0, "Test Company");
        await dbContext.Companies.AddAsync(company);
        await dbContext.SaveChangesAsync();

        var taxEntity = TaxEntity.Create(company.PublicId, "COMPANY", "Company Tax Entity");
        await dbContext.TaxEntities.AddAsync(taxEntity);
        await dbContext.SaveChangesAsync();

        company.UpdateTaxEntityId(taxEntity.Id);
        await dbContext.SaveChangesAsync();

        var user = User.Create("user@local", "hash", "salt");
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        // No CompanyUsers created intentionally for this test
        // No RoleAssignments created intentionally for this test

        return new CompanySeedContext(user.Id, organization.Id, company.Id, company.PublicId);
    }

    private static async Task<CompanySeedContext> SeedCompanyUserWithRoleAssignmentAsync(ErpDbContext dbContext)
    {
        var organization = Organization.Create(OrganizationType.Individual, "Company Org");
        await dbContext.Organizations.AddAsync(organization);
        await dbContext.SaveChangesAsync();

        var company = Company.Create(organization.Id, 0, "Test Company");
        await dbContext.Companies.AddAsync(company);
        await dbContext.SaveChangesAsync();

        var taxEntity = TaxEntity.Create(company.PublicId, "COMPANY", "Company Tax Entity");
        await dbContext.TaxEntities.AddAsync(taxEntity);
        await dbContext.SaveChangesAsync();

        company.UpdateTaxEntityId(taxEntity.Id);
        await dbContext.SaveChangesAsync();

        var user = User.Create("user@local", "hash", "salt");
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        // Create CompanyUsers membership
        var companyUser = CompanyUser.Create(company.PublicId, user.Id, CompanyUserStatus.Active);
        await dbContext.CompanyUsers.AddAsync(companyUser);
        await dbContext.SaveChangesAsync();

// Create permission and role
        var permission = Permission.Create(PermissionKeys.Inventory.StockRead, "Inventory Stock Read");
        var role = Role.Create("Company User", scopeType: RoleAssignmentScopeType.Company);
        await dbContext.Permissions.AddAsync(permission);
        await dbContext.Roles.AddAsync(role);
        await dbContext.SaveChangesAsync();

        await dbContext.RolePermissions.AddAsync(RolePermission.Create(role.Id, permission.Id));
        await dbContext.SaveChangesAsync();

        // Create role assignment
        await dbContext.RoleAssignments.AddAsync(RoleAssignment.CreateCompany(user.Id, role.Id, company.PublicId));
        await dbContext.SaveChangesAsync();

        return new CompanySeedContext(user.Id, organization.Id, company.Id, company.PublicId);
    }

    private static async Task<CompanySeedContext> SeedCompanyUserWithFeaturesAsync(ErpDbContext dbContext)
    {
        var context = await SeedCompanyUserWithRoleAssignmentAsync(dbContext);

        // Add company features
        var now = DateTime.UtcNow;
        var coreFeature = CompanyFeature.Create(context.CompanyId, FeatureCode.PharmaceuticalDrogueria.PublicId, true, now);
        var pharmacyFeature = CompanyFeature.Create(context.CompanyId, FeatureCode.PharmaceuticalBase.PublicId, true, now);
        await dbContext.CompanyFeatures.AddRangeAsync(coreFeature, pharmacyFeature);
        await dbContext.SaveChangesAsync();

        return context;
    }

    private static HttpClient CreateTenantClient(
        ApiWebApplicationFactory factory,
        long userId,
        long organizationId,
        long companyId,
        Guid companyPublicId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.OrganizationIdHeader, organizationId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.ScopeHeader, "tenant");
        client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyIdHeader, companyId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyPublicIdHeader, companyPublicId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, string.Empty);
        return client;
    }

    private sealed record PlatformSeedContext(long UserId, long OrganizationId, long CompanyId, Guid CompanyPublicId);

    private sealed record CompanySeedContext(long UserId, long OrganizationId, long CompanyId, Guid CompanyPublicId);
}
