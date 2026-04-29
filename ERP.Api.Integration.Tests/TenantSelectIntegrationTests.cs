using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using ERP.Api.Contracts.Identity;
using ERP.Modules.Identity.Contracts;
using ERP.Modules.Identity.Domain;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using ERP.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ERP.Api.Integration.Tests;

public sealed class TenantSelectIntegrationTests
{
    [Fact]
    public async Task SelectTenant_ReturnsUserInfoForSelectedCompany()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long organizationId;
        long currentCompanyId;
        Guid currentCompanyPublicId;
        long targetCompanyId;
        Guid targetCompanyPublicId;
        long userId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var organization = Organization.Create(OrganizationType.Individual, "Tenant Org");
            await dbContext.Organizations.AddAsync(organization);
            await dbContext.SaveChangesAsync();

            var currentCompany = Company.Create(organization.Id, 0, "Tenant Co");
            var targetCompany = Company.Create(organization.Id, 0, "Target Co");
            await dbContext.Companies.AddRangeAsync(currentCompany, targetCompany);
            await dbContext.SaveChangesAsync();

            var currentTaxEntity = TaxEntity.Create(currentCompany.PublicId, "TENANT-001", "Tenant Co");
            var targetTaxEntity = TaxEntity.Create(targetCompany.PublicId, "TENANT-002", "Target Co");
            await dbContext.TaxEntities.AddRangeAsync(currentTaxEntity, targetTaxEntity);
            await dbContext.SaveChangesAsync();

            currentCompany.UpdateTaxEntityId(currentTaxEntity.Id);
            targetCompany.UpdateTaxEntityId(targetTaxEntity.Id);
            await dbContext.SaveChangesAsync();

            var user = User.Create("user@tenant.local", "hash", "salt");
            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();

            var currentCompanyUser = CompanyUser.Create(currentCompany.PublicId, user.Id, CompanyUserStatus.Active);
            var targetCompanyUser = CompanyUser.Create(targetCompany.PublicId, user.Id, CompanyUserStatus.Active);
            await dbContext.CompanyUsers.AddRangeAsync(currentCompanyUser, targetCompanyUser);
            await dbContext.SaveChangesAsync();

            organizationId = organization.Id;
            currentCompanyId = currentCompany.Id;
            currentCompanyPublicId = currentCompany.PublicId;
            targetCompanyId = targetCompany.Id;
            targetCompanyPublicId = targetCompany.PublicId;
            userId = user.Id;
        }

        using var client = CreateClient(factory, userId, organizationId, currentCompanyId, currentCompanyPublicId);

        var response = await client.PostAsJsonAsync(
            "/api/tenant/select",
            new SelectTenantRequest(targetCompanyPublicId.ToString()));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var loginResult = await response.Content.ReadFromJsonAsync<LoginResult>();

        Assert.NotNull(loginResult);
        Assert.Equal(targetCompanyPublicId, loginResult!.User.CompanyPublicId);
        Assert.Equal(targetCompanyId, loginResult.User.CompanyId);
    }

    private static HttpClient CreateClient(
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
}
