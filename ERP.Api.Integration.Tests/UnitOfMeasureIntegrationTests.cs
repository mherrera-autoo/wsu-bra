using System.Linq;
using System.Net;
using System.Net.Http.Json;
using ERP.Api.Contracts.Companies;
using ERP.Modules.MasterData.Application.Services;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using ERP.Persistence;
using ERP.Shared.Application;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ERP.Api.Integration.Tests;

public sealed class UnitOfMeasureIntegrationTests
{
    [Fact]
    public async Task CannotSetTwoDefaults_ForSameDimension()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long companyId;
        Guid companyPublicId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (companyId, companyPublicId) = await SeedCompanyAsync(dbContext, "UOM Company");
        }

        using var client = CreateClient(factory, companyId, companyPublicId, RoleNames.PlatformSuperAdmin);

        using (var scope = factory.Services.CreateScope())
        {
            var seedService = scope.ServiceProvider.GetRequiredService<IUnitOfMeasureSeedService>();
            await seedService.SeedGlobalStandardUnitsAsync(CancellationToken.None);
            await seedService.EnableStandardUnitsForCompanyAsync(companyId, CancellationToken.None);
        }

        long boxUnitId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            boxUnitId = dbContext.UnitOfMeasures.Single(uom => uom.CanonicalCode == "box").Id;
        }

        var response = await client.PostAsJsonAsync(
            "/api/companies/uoms/defaults",
            new CompanyUnitOfMeasureDefaultRequest(UnitOfMeasureDimension.Count, boxUnitId));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ProductConversionRequired_ForCountBasedUnits()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());

        using var scope = factory.Services.CreateScope();
        var seedService = scope.ServiceProvider.GetRequiredService<IUnitOfMeasureSeedService>();
        var masterDataService = scope.ServiceProvider.GetRequiredService<MasterDataService>();
        var conversionService = scope.ServiceProvider.GetRequiredService<IUnitOfMeasureConversionService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();

        await seedService.SeedGlobalStandardUnitsAsync(CancellationToken.None);
        await seedService.EnableStandardUnitsForCompanyAsync(20, CancellationToken.None);

        var eaUnitId = dbContext.UnitOfMeasures.Single(uom => uom.CanonicalCode == "ea").Id;
        var boxUnitId = dbContext.UnitOfMeasures.Single(uom => uom.CanonicalCode == "box").Id;

        var productResult = await masterDataService.CreateProductAsync(
            20,
            "SKU-BOX",
            "Caja",
            "780000000020",
            eaUnitId,
            true,
            true,
            true,
            CancellationToken.None);
        Assert.True(productResult.Success);

        var conversionResult = await conversionService.ConvertAsync(
            productResult.Value!.Id,
            boxUnitId,
            eaUnitId,
            1m,
            CancellationToken.None);

        Assert.False(conversionResult.Success);
        Assert.Contains("Product-specific conversion", conversionResult.Error);
    }

    private static async Task<(long companyId, Guid companyPublicId)> SeedCompanyAsync(
        ErpDbContext dbContext,
        string name)
    {
        var organization = Organization.Create(OrganizationType.Individual, $"{name} Org");
        await dbContext.Organizations.AddAsync(organization);
        await dbContext.SaveChangesAsync();

        var company = Company.Create(organization.Id, 0, name);
        await dbContext.Companies.AddAsync(company);
        await dbContext.SaveChangesAsync();

        var taxEntity = TaxEntity.Create(company.PublicId, $"{name.ToUpperInvariant()}-UOM", name);
        await dbContext.TaxEntities.AddAsync(taxEntity);
        await dbContext.SaveChangesAsync();

        company.UpdateTaxEntityId(taxEntity.Id);
        await dbContext.SaveChangesAsync();

        return (company.Id, company.PublicId);
    }

    private static HttpClient CreateClient(ApiWebApplicationFactory factory, long companyId, Guid companyPublicId, params string[] roles)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, "1");
        client.DefaultRequestHeaders.Add(TestAuthHandler.OrganizationIdHeader, "1");
        client.DefaultRequestHeaders.Add(TestAuthHandler.ScopeHeader, "tenant");
        client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyIdHeader, companyId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyPublicIdHeader, companyPublicId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, string.Join(",", roles));
        return client;
    }
}
