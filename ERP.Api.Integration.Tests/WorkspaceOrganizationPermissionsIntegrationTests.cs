using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using ERP.Api.Contracts.Identity;
using ERP.Modules.Identity.Application.Security;
using ERP.Modules.Identity.Domain;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using ERP.Persistence;
using ERP.Shared.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ERP.Api.Integration.Tests;

public sealed class WorkspaceOrganizationPermissionsIntegrationTests
{
    private static HttpClient CreateClient(
        ApiWebApplicationFactory factory,
        long userId,
        long organizationId,
        long companyId,
        Guid companyPublicId,
        string scope = "tenant",
        string roles = "")
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.OrganizationIdHeader, organizationId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.ScopeHeader, scope);

        client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyIdHeader, companyId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyPublicIdHeader, companyPublicId.ToString());

        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, roles);
        return client;
    }

    [Fact]
    public async Task GetUserPermissions_WithJwtIndividualOrg_ShouldReturnWorkspacePermissions()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        using var factory = new ApiWebApplicationFactory(databaseName);
        
        long multiCompanyId, individualOrgId, userId;
        long companyId;
        Guid companyPublicId;
        
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            
            // Create organizations
            var multiCompanyOrg = Organization.Create(OrganizationType.MultiCompany, "MultiCompany Workspace");
            await dbContext.Organizations.AddAsync(multiCompanyOrg);
            
            var individualOrg = Organization.Create(OrganizationType.Individual, "Individual Tenant");
            await dbContext.Organizations.AddAsync(individualOrg);
            
            await dbContext.SaveChangesAsync();
            
            // Create user
            var user = User.Create("test@example.com", "password", "salt");
            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();
            
            // Create organization membership for workspace (Owner role)
            var workspaceMembership = OrganizationMember.Create(multiCompanyOrg.Id, user.Id, OrganizationRole.Owner);
            await dbContext.OrganizationMembers.AddAsync(workspaceMembership);
            
            // Create Organization-scoped RoleAssignment in workspace
            var role = Role.Create("Test Role", "Test Description", scopeType: RoleAssignmentScopeType.Organization);
            await dbContext.Roles.AddAsync(role);
            await dbContext.SaveChangesAsync();
            
            var permission = Permission.Create("test.permission", "Test Permission");
            await dbContext.Permissions.AddAsync(permission);
            await dbContext.SaveChangesAsync();
            
            var rolePermission = RolePermission.Create(role.Id, permission.Id);
            await dbContext.RolePermissions.AddAsync(rolePermission);
            await dbContext.SaveChangesAsync();
            
            var roleAssignment = RoleAssignment.CreateOrganization(user.Id, role.Id, multiCompanyOrg.Id);
            await dbContext.RoleAssignments.AddAsync(roleAssignment);
            
            await dbContext.SaveChangesAsync();
            
            multiCompanyId = multiCompanyOrg.Id;
            individualOrgId = individualOrg.Id;
            userId = user.Id;

            (companyId, companyPublicId) = await SeedCompanyAsync(dbContext, individualOrg.Id, "Individual Tenant Co");
        }

        // Act - Simulate JWT with Individual Organization (tenant)
        using var client = CreateClient(factory, userId, individualOrgId, companyId, companyPublicId);
        var response = await client.GetAsync("/api/identity/me/permissions");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var permissionsResponse = await response.Content.ReadFromJsonAsync<UserPermissionsResponse>();
        Assert.NotNull(permissionsResponse);
        
        // Should have organization permissions from workspace, not empty
        Assert.NotEmpty(permissionsResponse.Organization);
        Assert.Contains("test.permission", permissionsResponse.Organization);
        
        // Context should show workspace organization ID, not JWT individual org
        Assert.Equal(multiCompanyId, permissionsResponse.Context.OrganizationId);
        Assert.NotEqual(individualOrgId, permissionsResponse.Context.OrganizationId);
    }

    [Fact]
    public async Task GetUserPermissions_WithJwtMultiCompanyOrg_ShouldReturnSameOrg()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        using var factory = new ApiWebApplicationFactory(databaseName);
        
        long multiCompanyId, userId;
        long companyId;
        Guid companyPublicId;
        
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            
            var multiCompanyOrg = Organization.Create(OrganizationType.MultiCompany, "MultiCompany Workspace");
            await dbContext.Organizations.AddAsync(multiCompanyOrg);
            
            var user = User.Create("test@example.com", "password", "salt");
            await dbContext.Users.AddAsync(user);
            
            var workspaceMembership = OrganizationMember.Create(multiCompanyOrg.Id, user.Id, OrganizationRole.Owner);
            await dbContext.OrganizationMembers.AddAsync(workspaceMembership);
            
            var role = Role.Create("Test Role", "Test Description", scopeType: RoleAssignmentScopeType.Organization);
            await dbContext.Roles.AddAsync(role);
            await dbContext.SaveChangesAsync();
            
            var permission = Permission.Create("test.permission", "Test Permission");
            await dbContext.Permissions.AddAsync(permission);
            await dbContext.SaveChangesAsync();
            
            var rolePermission = RolePermission.Create(role.Id, permission.Id);
            await dbContext.RolePermissions.AddAsync(rolePermission);
            await dbContext.SaveChangesAsync();
            
            var roleAssignment = RoleAssignment.CreateOrganization(user.Id, role.Id, multiCompanyOrg.Id);
            await dbContext.RoleAssignments.AddAsync(roleAssignment);
            
            await dbContext.SaveChangesAsync();
            
            multiCompanyId = multiCompanyOrg.Id;
            userId = user.Id;

            (companyId, companyPublicId) = await SeedCompanyAsync(dbContext, multiCompanyOrg.Id, "Workspace Co");
        }

        // Act - JWT with MultiCompany Organization
        using var client = CreateClient(factory, userId, multiCompanyId, companyId, companyPublicId);
        var response = await client.GetAsync("/api/identity/me/permissions");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var permissionsResponse = await response.Content.ReadFromJsonAsync<UserPermissionsResponse>();
        Assert.NotNull(permissionsResponse);
        
        Assert.NotEmpty(permissionsResponse.Organization);
        Assert.Equal(multiCompanyId, permissionsResponse.Context.OrganizationId);
    }

    [Fact]
    public async Task GetUserPermissions_WithNoMultiCompanyMembership_ShouldReturnEmptyOrgPermissions()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        using var factory = new ApiWebApplicationFactory(databaseName);
        
        long individualOrgId, userId;
        long companyId;
        Guid companyPublicId;
        
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            
            var individualOrg = Organization.Create(OrganizationType.Individual, "Individual Only");
            await dbContext.Organizations.AddAsync(individualOrg);
            
            var user = User.Create("test@example.com", "password", "salt");
            await dbContext.Users.AddAsync(user);
            
            // Membership only in Individual org, no MultiCompany
            var membership = OrganizationMember.Create(individualOrg.Id, user.Id, OrganizationRole.Owner);
            await dbContext.OrganizationMembers.AddAsync(membership);
            
            await dbContext.SaveChangesAsync();
            
            individualOrgId = individualOrg.Id;
            userId = user.Id;

            (companyId, companyPublicId) = await SeedCompanyAsync(dbContext, individualOrg.Id, "Individual Only Co");
        }

        // Act
        using var client = CreateClient(factory, userId, individualOrgId, companyId, companyPublicId);
        var response = await client.GetAsync("/api/identity/me/permissions");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var permissionsResponse = await response.Content.ReadFromJsonAsync<UserPermissionsResponse>();
        Assert.NotNull(permissionsResponse);
        
        // Empty organization permissions since no MultiCompany workspace
        Assert.Empty(permissionsResponse.Organization);
        Assert.Null(permissionsResponse.Context.OrganizationId);
    }

    private static async Task<(long companyId, Guid companyPublicId)> SeedCompanyAsync(
        ErpDbContext dbContext,
        long organizationId,
        string name)
    {
        var company = Company.Create(organizationId, 0, name);
        await dbContext.Companies.AddAsync(company);
        await dbContext.SaveChangesAsync();

        var taxEntity = TaxEntity.Create(company.PublicId, $"{name.ToUpperInvariant()}-TAX", name);
        await dbContext.TaxEntities.AddAsync(taxEntity);
        await dbContext.SaveChangesAsync();

        company.UpdateTaxEntityId(taxEntity.Id);
        await dbContext.SaveChangesAsync();

        return (company.Id, company.PublicId);
    }
}
