using System.Net;
using System.Net.Http.Json;
using ERP.Api.Contracts.Companies;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using ERP.Persistence;
using ERP.Shared.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ERP.Api.Integration.Tests;

public sealed class CompanyFeatureEndpointsTests
{
    [Fact]
    public async Task CompanyAdmin_CannotManageFeatures()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long companyId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            companyId = await SeedCompanyAsync(dbContext, "Feature Test Co");
        }

        using var client = CreateClient(factory, companyId, RoleNames.CompanyAdmin);

        var okResponse = await client.GetAsync("/api/companies/features");
        var enableResponse = await client.PostAsJsonAsync(
            $"/api/companies/features/{FeatureCode.PharmaceuticalBase.PublicId}:enable",
            CreateToggleRequest("enable by company admin"));
        var disableResponse = await client.PostAsJsonAsync(
            $"/api/companies/features/{FeatureCode.PharmaceuticalBase.PublicId}:disable",
            CreateToggleRequest("disable by company admin"));

        Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, enableResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, disableResponse.StatusCode);
    }

    [Fact]
    public async Task FeaturePackAdder_CanEnableAndDisable()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long companyId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            companyId = await SeedCompanyAsync(dbContext, "Feature Pack Co");
        }

        using var client = CreateClient(factory, companyId, RoleNames.FeaturePackAdder);

        var getResponse = await client.GetAsync("/api/companies/features");
        var enableResponse = await client.PostAsJsonAsync(
            $"/api/companies/features/{FeatureCode.PharmaceuticalBase.PublicId}:enable",
            CreateToggleRequest("enable by feature pack adder"));
        var disableResponse = await client.PostAsJsonAsync(
            $"/api/companies/features/{FeatureCode.PharmaceuticalBase.PublicId}:disable",
            CreateToggleRequest("disable by feature pack adder"));

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, enableResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, disableResponse.StatusCode);
    }

    [Fact]
    public async Task PlatformSuperAdmin_CanEnableAndDisable()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long companyId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            companyId = await SeedCompanyAsync(dbContext, "Feature Admin Co");
        }

        using var client = CreateClient(factory, companyId, RoleNames.PlatformSuperAdmin);

        var enableResponse = await client.PostAsJsonAsync(
            $"/api/companies/features/{FeatureCode.PharmaceuticalBase.PublicId}:enable",
            CreateToggleRequest("enable by superadmin"));
        var disableResponse = await client.PostAsJsonAsync(
            $"/api/companies/features/{FeatureCode.PharmaceuticalBase.PublicId}:disable",
            CreateToggleRequest("disable by superadmin"));

        Assert.Equal(HttpStatusCode.NoContent, enableResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, disableResponse.StatusCode);
    }

    [Fact]
    public async Task EnableDisable_IsIdempotent_AndAudited()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long companyId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            companyId = await SeedCompanyAsync(dbContext, "Feature Audit Co");
        }

        using var client = CreateClient(factory, companyId, RoleNames.PlatformSuperAdmin);

        var enableFirst = await client.PostAsJsonAsync(
            $"/api/companies/features/{FeatureCode.PharmaceuticalDispensing.PublicId}:enable",
            CreateToggleRequest("first enable"));
        var enableSecond = await client.PostAsJsonAsync(
            $"/api/companies/features/{FeatureCode.PharmaceuticalDispensing.PublicId}:enable",
            CreateToggleRequest("second enable"));

        Assert.Equal(HttpStatusCode.NoContent, enableFirst.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, enableSecond.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var featureCount = await dbContext.CompanyFeatures.CountAsync();
            var enabledAuditCount = await dbContext.CompanyFeatureAudits
                .CountAsync(audit => audit.Action == CompanyFeatureAuditAction.Enabled);

            Assert.Equal(1, featureCount);
            Assert.Equal(2, enabledAuditCount);
        }

        var disableFirst = await client.PostAsJsonAsync(
            $"/api/companies/features/{FeatureCode.PharmaceuticalDispensing.PublicId}:disable",
            CreateToggleRequest("first disable"));
        var disableSecond = await client.PostAsJsonAsync(
            $"/api/companies/features/{FeatureCode.PharmaceuticalDispensing.PublicId}:disable",
            CreateToggleRequest("second disable"));

        Assert.Equal(HttpStatusCode.NoContent, disableFirst.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, disableSecond.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var featureCount = await dbContext.CompanyFeatures.CountAsync();
            var activeFeatureCount = await dbContext.CompanyFeatures.CountAsync(feature => feature.IsActive);
            var disabledAuditCount = await dbContext.CompanyFeatureAudits
                .CountAsync(audit => audit.Action == CompanyFeatureAuditAction.Disabled);

            Assert.Equal(1, featureCount);
            Assert.Equal(0, activeFeatureCount);
            Assert.Equal(2, disabledAuditCount);
        }
    }

    [Fact]
    public async Task Cache_RefreshImmediately()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long companyId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            companyId = await SeedCompanyAsync(dbContext, "Feature Cache Co");
        }

        using var client = CreateClient(factory, companyId, RoleNames.PlatformSuperAdmin);

        using var featureScope = factory.Services.CreateScope();
        var featureService = featureScope.ServiceProvider.GetRequiredService<IFeatureService>();

        var initiallyEnabled = await featureService.IsEnabled(companyId, FeatureKeys.PharmaceuticalBase);
        Assert.False(initiallyEnabled);

        var enableResponse = await client.PostAsJsonAsync(
            $"/api/companies/features/{FeatureCode.PharmaceuticalBase.PublicId}:enable",
            CreateToggleRequest("enable for cache"));
        Assert.Equal(HttpStatusCode.NoContent, enableResponse.StatusCode);

        Assert.True(await featureService.IsEnabled(companyId, FeatureKeys.PharmaceuticalBase));

        var disableResponse = await client.PostAsJsonAsync(
            $"/api/companies/features/{FeatureCode.PharmaceuticalBase.PublicId}:disable",
            CreateToggleRequest("disable for cache"));
        Assert.Equal(HttpStatusCode.NoContent, disableResponse.StatusCode);

        Assert.False(await featureService.IsEnabled(companyId, FeatureKeys.PharmaceuticalBase));
    }

    private static CompanyFeatureToggleRequest CreateToggleRequest(string reason)
    {
        return new CompanyFeatureToggleRequest(reason, Guid.NewGuid());
    }

    private static async Task<long> SeedCompanyAsync(ErpDbContext dbContext, string name)
    {
        var organization = Organization.Create(OrganizationType.Individual, $"{name} Org");
        await dbContext.Organizations.AddAsync(organization);
        await dbContext.SaveChangesAsync();

        var company = Company.Create(organization.Id, 0, name);
        await dbContext.Companies.AddAsync(company);
        await dbContext.SaveChangesAsync();

        var taxEntity = TaxEntity.Create(company.PublicId, $"{name.ToUpperInvariant()}-TAX", name);
        await dbContext.TaxEntities.AddAsync(taxEntity);
        await dbContext.SaveChangesAsync();

        company.UpdateTaxEntityId(taxEntity.Id);
        await dbContext.SaveChangesAsync();

        return company.Id;
    }

    private static HttpClient CreateClient(ApiWebApplicationFactory factory, long companyId, params string[] roles)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, "1");
        client.DefaultRequestHeaders.Add(TestAuthHandler.OrganizationIdHeader, "1");
        client.DefaultRequestHeaders.Add(TestAuthHandler.ScopeHeader, "platform");
        client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyIdHeader, companyId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, string.Join(",", roles));
        return client;
    }
}
