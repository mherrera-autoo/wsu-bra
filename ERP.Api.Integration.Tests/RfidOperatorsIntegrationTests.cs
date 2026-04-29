using System.Globalization;
using System.Net;
using System.Text.Json;
using ERP.Modules.Identity.Domain;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using ERP.Modules.Rfid.Domain;
using ERP.Persistence;
using ERP.Shared.Application;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ERP.Api.Integration.Tests;

public sealed class RfidOperatorsIntegrationTests
{
    [Fact]
    public async Task ListOperators_WithoutCompanyPublicId_UsesTokenCompany()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        CompanySeed companyA;
        CompanySeed companyB;
        long userId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var organization = await SeedOrganizationAsync(dbContext, "RFID Org");
            companyA = await SeedCompanyAsync(dbContext, organization.Id, "Company A", "TAX-A");
            companyB = await SeedCompanyAsync(dbContext, organization.Id, "Company B", "TAX-B");
            userId = await SeedUserAsync(dbContext, "rfid-operators-1@test.local");

            await SeedOperatorAsync(dbContext, companyA.CompanyPublicId, "OP-A", "Operator A");
            await SeedOperatorAsync(dbContext, companyB.CompanyPublicId, "OP-B", "Operator B");
        }

        using var client = CreateClient(factory, companyA, userId);

        var response = await client.GetAsync("/api/rfid/operators?Page=1&PageSize=200");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        Assert.Equal(1, document.RootElement.GetProperty("total").GetInt32());
        Assert.Equal("Operator A", document.RootElement.GetProperty("items")[0].GetProperty("fullName").GetString());
    }

    [Fact]
    public async Task ListOperators_WithCompanyPublicIdEqualToToken_ReturnsOk()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        CompanySeed company;
        long userId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var organization = await SeedOrganizationAsync(dbContext, "RFID Org");
            company = await SeedCompanyAsync(dbContext, organization.Id, "Company A", "TAX-A");
            userId = await SeedUserAsync(dbContext, "rfid-operators-2@test.local");

            await SeedOperatorAsync(dbContext, company.CompanyPublicId, "OP-A", "Operator A");
        }

        using var client = CreateClient(factory, company, userId);

        var response = await client.GetAsync($"/api/rfid/operators?CompanyPublicId={company.CompanyPublicId}&Page=1&PageSize=200");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ListOperators_WithDifferentCompanyPublicId_WithoutPlatformPermission_ReturnsForbidden()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        CompanySeed companyA;
        CompanySeed companyB;
        long userId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var organization = await SeedOrganizationAsync(dbContext, "RFID Org");
            companyA = await SeedCompanyAsync(dbContext, organization.Id, "Company A", "TAX-A");
            companyB = await SeedCompanyAsync(dbContext, organization.Id, "Company B", "TAX-B");
            userId = await SeedUserAsync(dbContext, "rfid-operators-3@test.local");

            await SeedOperatorAsync(dbContext, companyB.CompanyPublicId, "OP-B", "Operator B");
        }

        using var client = CreateClient(factory, companyA, userId);

        var response = await client.GetAsync($"/api/rfid/operators?CompanyPublicId={companyB.CompanyPublicId}&Page=1&PageSize=200");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ListOperators_WithDifferentCompanyPublicId_WithPlatformPermission_ReturnsRequestedCompanyOperators()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        CompanySeed companyA;
        CompanySeed companyB;
        long userId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var organization = await SeedOrganizationAsync(dbContext, "RFID Org");
            companyA = await SeedCompanyAsync(dbContext, organization.Id, "Company A", "TAX-A");
            companyB = await SeedCompanyAsync(dbContext, organization.Id, "Company B", "TAX-B");
            userId = await SeedUserAsync(dbContext, "rfid-operators-4@test.local");

            await SeedOperatorAsync(dbContext, companyA.CompanyPublicId, "OP-A", "Operator A");
            await SeedOperatorAsync(dbContext, companyB.CompanyPublicId, "OP-B", "Operator B");
            await GrantWsuManagePermissionAsync(dbContext, userId);
        }

        using var client = CreateClient(factory, companyA, userId);

        var response = await client.GetAsync($"/api/rfid/operators?CompanyPublicId={companyB.CompanyPublicId}&Page=1&PageSize=200");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        Assert.Equal(1, document.RootElement.GetProperty("total").GetInt32());
        Assert.Equal("Operator B", document.RootElement.GetProperty("items")[0].GetProperty("fullName").GetString());
    }

    [Fact]
    public async Task ListOperators_WithEmptyCompanyPublicId_ReturnsBadRequest()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        CompanySeed company;
        long userId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var organization = await SeedOrganizationAsync(dbContext, "RFID Org");
            company = await SeedCompanyAsync(dbContext, organization.Id, "Company A", "TAX-A");
            userId = await SeedUserAsync(dbContext, "rfid-operators-5@test.local");
        }

        using var client = CreateClient(factory, company, userId);

        var response = await client.GetAsync($"/api/rfid/operators?CompanyPublicId={Guid.Empty}&Page=1&PageSize=200");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static HttpClient CreateClient(ApiWebApplicationFactory factory, CompanySeed company, long userId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString(CultureInfo.InvariantCulture));
        client.DefaultRequestHeaders.Add(TestAuthHandler.OrganizationIdHeader, company.OrganizationId.ToString(CultureInfo.InvariantCulture));
        client.DefaultRequestHeaders.Add(TestAuthHandler.ScopeHeader, "tenant");
        client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyIdHeader, company.CompanyId.ToString(CultureInfo.InvariantCulture));
        client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyPublicIdHeader, company.CompanyPublicId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, string.Empty);
        return client;
    }

    private static async Task<Organization> SeedOrganizationAsync(ErpDbContext dbContext, string name)
    {
        var organization = Organization.Create(OrganizationType.Individual, name);
        await dbContext.Organizations.AddAsync(organization);
        await dbContext.SaveChangesAsync();
        return organization;
    }

    private static async Task<CompanySeed> SeedCompanyAsync(ErpDbContext dbContext, long organizationId, string name, string taxId)
    {
        var company = Company.Create(organizationId, 0, name);
        await dbContext.Companies.AddAsync(company);
        await dbContext.SaveChangesAsync();

        var taxEntity = TaxEntity.Create(company.PublicId, taxId, name);
        await dbContext.TaxEntities.AddAsync(taxEntity);
        await dbContext.SaveChangesAsync();

        company.UpdateTaxEntityId(taxEntity.Id);
        await dbContext.SaveChangesAsync();

        return new CompanySeed(organizationId, company.Id, company.PublicId);
    }

    private static async Task<long> SeedUserAsync(ErpDbContext dbContext, string email)
    {
        var user = User.Create(email, "hash", "salt");
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
        return user.Id;
    }

    private static async Task SeedOperatorAsync(ErpDbContext dbContext, Guid companyPublicId, string code, string fullName)
    {
        var op = Operator.Create(companyPublicId, code, fullName, "11.111.111-1", isActive: true);
        await dbContext.Operators.AddAsync(op);
        await dbContext.SaveChangesAsync();
    }

    private static async Task GrantWsuManagePermissionAsync(ErpDbContext dbContext, long userId)
    {
        var role = Role.Create("WSU Platform Operator", scopeType: RoleAssignmentScopeType.Platform);
        await dbContext.Roles.AddAsync(role);
        await dbContext.SaveChangesAsync();

        var permission = Permission.Create(PermissionKeys.Platform.WsuManage, PermissionKeys.Platform.WsuManage);
        await dbContext.Permissions.AddAsync(permission);
        await dbContext.SaveChangesAsync();

        await dbContext.RolePermissions.AddAsync(RolePermission.Create(role.Id, permission.Id));
        await dbContext.RoleAssignments.AddAsync(RoleAssignment.CreatePlatform(userId, role.Id));
        await dbContext.SaveChangesAsync();
    }

    private sealed record CompanySeed(long OrganizationId, long CompanyId, Guid CompanyPublicId);
}
