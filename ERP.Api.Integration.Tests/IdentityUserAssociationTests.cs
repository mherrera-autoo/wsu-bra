using System.Net;
using System.Net.Http.Json;
using ERP.Api.Contracts.Identity;
using ERP.Modules.Identity.Contracts;
using ERP.Modules.Identity.Domain;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using ERP.Persistence;
using ERP.Shared.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ERP.Api.Integration.Tests;

public sealed class IdentityUserAssociationTests
{
    [Fact]
    public async Task AssociateUserToCompany_CreatesWorkspaceMembership_AndActiveCompanyUser()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long organizationId;
        Guid companyPublicId;
        Guid userPublicId;
        long userId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();

            var organization = Organization.Create(OrganizationType.Individual, "Tenant Org");
            await dbContext.Organizations.AddAsync(organization);
            await dbContext.SaveChangesAsync();

            var company = Company.Create(organization.Id, 0, "Tenant Co");
            await dbContext.Companies.AddAsync(company);
            await dbContext.SaveChangesAsync();

            var taxEntity = TaxEntity.Create(company.PublicId, "12345678-9", "Tenant Co");
            await dbContext.TaxEntities.AddAsync(taxEntity);
            await dbContext.SaveChangesAsync();

            company.UpdateTaxEntityId(taxEntity.Id);
            await dbContext.SaveChangesAsync();

            var user = User.Create("associate@tenant.local", "hash", "salt");
            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();

            organizationId = organization.Id;
            companyPublicId = company.PublicId;
            userPublicId = user.PublicId;
            userId = user.Id;
        }

        using var client = CreateClient(factory);
        var response = await client.PostAsJsonAsync(
            $"/api/identity/users/{userPublicId}/companies",
            new AssociateUserToCompanyRequest(companyPublicId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var verificationScope = factory.Services.CreateScope();
        var verifyDb = verificationScope.ServiceProvider.GetRequiredService<ErpDbContext>();

        var organizationMember = await verifyDb.OrganizationMembers
            .SingleAsync(member => member.OrganizationId == organizationId && member.UserId == userId);
        var companyUser = await verifyDb.CompanyUsers
            .SingleAsync(member => member.CompanyPublicId == companyPublicId && member.UserId == userId);
        var companyLink = await verifyDb.CompanyLinks
            .SingleAsync(link => link.OrganizationId == organizationId && link.CompanyPublicId == companyPublicId);
        var ownerAssignment = await verifyDb.RoleAssignments
            .Include(assignment => assignment.Role)
            .SingleAsync(assignment => assignment.UserId == userId && assignment.CompanyPublicId == companyPublicId);

        Assert.Equal(OrganizationRole.Member, organizationMember.Role);
        Assert.Equal(CompanyUserStatus.Active, companyUser.Status);
        Assert.Equal(CompanyLinkAccessType.ExternalAccountant, companyLink.AccessType);
        Assert.Equal(RoleNames.CompanyOwner, ownerAssignment.Role.Name);
        Assert.Equal(RoleAssignmentStatus.Active, ownerAssignment.Status);
    }

    [Fact]
    public async Task AssociateUserToCompany_CreatesCompanyLinkInAssociatedUserWorkspace()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long workspaceOrganizationId;
        long tenantOrganizationId;
        Guid companyPublicId;
        Guid userPublicId;
        long userId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();

            var user = User.Create("workspace@tenant.local", "hash", "salt");
            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();

            var workspaceOrganization = Organization.Create(OrganizationType.MultiCompany, "Workspace Org");
            await dbContext.Organizations.AddAsync(workspaceOrganization);
            await dbContext.SaveChangesAsync();

            await dbContext.OrganizationMembers.AddAsync(
                OrganizationMember.Create(workspaceOrganization.Id, user.Id, OrganizationRole.Member));
            await dbContext.SaveChangesAsync();

            var tenantOrganization = Organization.Create(OrganizationType.Individual, "Tenant Org");
            await dbContext.Organizations.AddAsync(tenantOrganization);
            await dbContext.SaveChangesAsync();

            var company = Company.Create(tenantOrganization.Id, 0, "Tenant Co");
            await dbContext.Companies.AddAsync(company);
            await dbContext.SaveChangesAsync();

            var taxEntity = TaxEntity.Create(company.PublicId, "22222222-2", "Tenant Co");
            await dbContext.TaxEntities.AddAsync(taxEntity);
            await dbContext.SaveChangesAsync();

            company.UpdateTaxEntityId(taxEntity.Id);
            await dbContext.SaveChangesAsync();

            workspaceOrganizationId = workspaceOrganization.Id;
            tenantOrganizationId = tenantOrganization.Id;
            companyPublicId = company.PublicId;
            userPublicId = user.PublicId;
            userId = user.Id;
        }

        using var client = CreateClient(factory);
        var response = await client.PostAsJsonAsync(
            $"/api/identity/users/{userPublicId}/companies",
            new AssociateUserToCompanyRequest(companyPublicId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var verificationScope = factory.Services.CreateScope();
        var verifyDb = verificationScope.ServiceProvider.GetRequiredService<ErpDbContext>();

        var workspaceLink = await verifyDb.CompanyLinks
            .SingleAsync(link => link.OrganizationId == workspaceOrganizationId && link.CompanyPublicId == companyPublicId);
        var tenantLinkCount = await verifyDb.CompanyLinks
            .CountAsync(link => link.OrganizationId == tenantOrganizationId && link.CompanyPublicId == companyPublicId);

        Assert.Equal(CompanyLinkAccessType.ExternalAccountant, workspaceLink.AccessType);
        Assert.Equal(0, tenantLinkCount);
    }

    [Fact]
    public async Task AssociateUserToCompany_ReactivatesExistingCompanyMembership()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        Guid companyPublicId;
        Guid userPublicId;
        long userId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();

            var organization = Organization.Create(OrganizationType.Individual, "Tenant Org");
            await dbContext.Organizations.AddAsync(organization);
            await dbContext.SaveChangesAsync();

            var company = Company.Create(organization.Id, 0, "Tenant Co");
            await dbContext.Companies.AddAsync(company);
            await dbContext.SaveChangesAsync();

            var taxEntity = TaxEntity.Create(company.PublicId, "10987654-3", "Tenant Co");
            await dbContext.TaxEntities.AddAsync(taxEntity);
            await dbContext.SaveChangesAsync();

            company.UpdateTaxEntityId(taxEntity.Id);
            await dbContext.SaveChangesAsync();

            var user = User.Create("suspended@tenant.local", "hash", "salt");
            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();

            var suspendedMembership = CompanyUser.Create(company.PublicId, user.Id, CompanyUserStatus.Suspended);
            await dbContext.CompanyUsers.AddAsync(suspendedMembership);

            var companyOwnerRole = Role.Create(RoleNames.CompanyOwner, isSystem: true, scopeType: RoleAssignmentScopeType.Company);
            await dbContext.Roles.AddAsync(companyOwnerRole);
            await dbContext.SaveChangesAsync();

            var inactiveOwnerAssignment = RoleAssignment.CreateCompany(user.Id, companyOwnerRole.Id, company.PublicId);
            inactiveOwnerAssignment.Deactivate();
            await dbContext.RoleAssignments.AddAsync(inactiveOwnerAssignment);
            await dbContext.SaveChangesAsync();

            companyPublicId = company.PublicId;
            userPublicId = user.PublicId;
            userId = user.Id;
        }

        using var client = CreateClient(factory);
        var response = await client.PostAsJsonAsync(
            $"/api/identity/users/{userPublicId}/companies",
            new AssociateUserToCompanyRequest(companyPublicId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var verificationScope = factory.Services.CreateScope();
        var verifyDb = verificationScope.ServiceProvider.GetRequiredService<ErpDbContext>();

        var companyUser = await verifyDb.CompanyUsers
            .SingleAsync(member => member.CompanyPublicId == companyPublicId && member.UserId == userId);
        var companyLink = await verifyDb.CompanyLinks
            .SingleAsync(link => link.CompanyPublicId == companyPublicId && link.OrganizationId > 0);
        var ownerAssignment = await verifyDb.RoleAssignments
            .Include(assignment => assignment.Role)
            .SingleAsync(assignment => assignment.UserId == userId && assignment.CompanyPublicId == companyPublicId);

        Assert.Equal(CompanyUserStatus.Active, companyUser.Status);
        Assert.Equal(CompanyLinkAccessType.ExternalAccountant, companyLink.AccessType);
        Assert.Equal(RoleNames.CompanyOwner, ownerAssignment.Role.Name);
        Assert.Equal(RoleAssignmentStatus.Active, ownerAssignment.Status);
    }

    private static HttpClient CreateClient(ApiWebApplicationFactory factory)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, "1");
        client.DefaultRequestHeaders.Add(TestAuthHandler.OrganizationIdHeader, "1");
        client.DefaultRequestHeaders.Add(TestAuthHandler.ScopeHeader, "tenant");
        client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyIdHeader, "1");
        client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyPublicIdHeader, Guid.NewGuid().ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, string.Empty);
        return client;
    }
}
