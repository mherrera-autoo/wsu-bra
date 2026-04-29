using System;
using System.Net;
using System.Net.Http;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using ERP.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ERP.Api.Integration.Tests;

public sealed class TenantGuardTests
{
    [Fact]
    public async Task TenantScope_AllowsTenantEndpoints()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long organizationId;
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

            var taxEntity = TaxEntity.Create(company.PublicId, "TENANT-001", "Tenant Co");
            await dbContext.TaxEntities.AddAsync(taxEntity);
            await dbContext.SaveChangesAsync();

            company.UpdateTaxEntityId(taxEntity.Id);
            await dbContext.SaveChangesAsync();

            organizationId = organization.Id;
            companyId = company.Id;
            companyPublicId = company.PublicId;
        }

        using var client = CreateClient(factory, organizationId, scope: "tenant", companyId: companyId, companyPublicId: companyPublicId);

        var response = await client.GetAsync("/api/identity/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PlatformScope_RejectsTenantEndpoints()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        using var client = CreateClient(factory, organizationId: 1, scope: "platform", companyId: null, companyPublicId: null);

        var response = await client.GetAsync("/api/identity/me");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static HttpClient CreateClient(
        ApiWebApplicationFactory factory,
        long organizationId,
        string scope,
        long? companyId,
        Guid? companyPublicId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, "1");
        client.DefaultRequestHeaders.Add(TestAuthHandler.OrganizationIdHeader, organizationId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.ScopeHeader, scope);
        if (companyId.HasValue)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyIdHeader, companyId.Value.ToString());
        }
        if (companyPublicId.HasValue)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyPublicIdHeader, companyPublicId.Value.ToString());
        }

        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, string.Empty);
        return client;
    }
}
