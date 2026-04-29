using ERP.Api.Contracts.Identity;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using ERP.Persistence;
using ERP.Shared.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ERP.Api.Integration.Tests;

public sealed class IdentityUserCreationTests
{
    [Fact]
    public async Task CreateUser_ReturnsForbidden_WhenCompanyMissing()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        using var client = CreateClient(factory, companyId: 999, companyPublicId: Guid.NewGuid());

        var response = await client.PostAsJsonAsync(
            "/api/identity/users",
            new CreateUserRequest("missing@tenant.local", "Password!123"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
        Assert.Equal(0, await dbContext.Users.CountAsync());
        Assert.Equal(0, await dbContext.CompanyUsers.CountAsync());
    }

    [Fact]
    public async Task CreateUser_CreatesUser_AndCompanyUser_WhenCompanyExists()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long companyId;
        Guid companyPublicId;
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
            companyId = company.Id;
            companyPublicId = company.PublicId;
        }

        using var client = CreateClient(factory, companyId, companyPublicId);
        var response = await client.PostAsJsonAsync(
            "/api/identity/users",
            new CreateUserRequest("user@tenant.local", "Password!123"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var verificationScope = factory.Services.CreateScope();
        var verifyDb = verificationScope.ServiceProvider.GetRequiredService<ErpDbContext>();
        var user = await verifyDb.Users.SingleAsync();
        var companyUser = await verifyDb.CompanyUsers.SingleAsync();

        Assert.Equal(companyPublicId, companyUser.CompanyPublicId);
        Assert.Equal(user.Id, companyUser.UserId);
        Assert.Equal("user@tenant.local", user.Email);
    }

    private static HttpClient CreateClient(ApiWebApplicationFactory factory, long companyId, Guid? companyPublicId = null)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, "1");
        client.DefaultRequestHeaders.Add(TestAuthHandler.OrganizationIdHeader, "1");
        client.DefaultRequestHeaders.Add(TestAuthHandler.ScopeHeader, "tenant");
        client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyIdHeader, companyId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyPublicIdHeader, (companyPublicId ?? Guid.NewGuid()).ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, string.Empty);
        return client;
    }
}
