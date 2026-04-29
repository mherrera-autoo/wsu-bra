using System.Linq;
using ERP.Modules.MasterData.Application.Services;
using ERP.Modules.MasterData.Domain;
using Xunit;

namespace ERP.Modules.MasterData.WhiteBox.Tests;

public sealed class UnitOfMeasureSeedServiceTests
{
    [Fact]
    public async Task SeedGlobalStandardUnitsAsync_ShouldBeIdempotent()
    {
        var unitRepository = new InMemoryUnitOfMeasureRepository();
        var translationRepository = new InMemoryUnitOfMeasureTranslationRepository();
        var mappingRepository = new InMemoryUnitOfMeasureExternalMappingRepository();
        var companyUnitRepository = new InMemoryCompanyUnitOfMeasureRepository();
        var productRepository = new InMemoryProductRepository();
        var unitOfWork = new InMemoryUnitOfWork();

        var catalog = new UnitOfMeasureCatalogService(unitRepository, unitOfWork);
        var companyService = new CompanyUnitOfMeasureService(companyUnitRepository, productRepository, unitRepository, unitOfWork);
        var seedService = new UnitOfMeasureSeedService(catalog, translationRepository, mappingRepository, companyService, unitOfWork);

        var first = await seedService.SeedGlobalStandardUnitsAsync(CancellationToken.None);
        var second = await seedService.SeedGlobalStandardUnitsAsync(CancellationToken.None);

        Assert.True(first.Success);
        Assert.True(second.Success);
        Assert.Equal(first.Value!.Count, second.Value!.Count);
    }

    [Fact]
    public async Task SeedGlobalStandardUnitsAsync_ShouldUpdateMissingValues()
    {
        var unitRepository = new InMemoryUnitOfMeasureRepository();
        var translationRepository = new InMemoryUnitOfMeasureTranslationRepository();
        var mappingRepository = new InMemoryUnitOfMeasureExternalMappingRepository();
        var companyUnitRepository = new InMemoryCompanyUnitOfMeasureRepository();
        var productRepository = new InMemoryProductRepository();
        var unitOfWork = new InMemoryUnitOfWork();

        var catalog = new UnitOfMeasureCatalogService(unitRepository, unitOfWork);
        var companyService = new CompanyUnitOfMeasureService(companyUnitRepository, productRepository, unitRepository, unitOfWork);
        var seedService = new UnitOfMeasureSeedService(catalog, translationRepository, mappingRepository, companyService, unitOfWork);

        var existing = UnitOfMeasure.Create("ea", "EA", UnitOfMeasureDimension.Count, false, 1m, 0, true, null);
        await unitRepository.AddAsync(existing, CancellationToken.None);

        var result = await seedService.SeedGlobalStandardUnitsAsync(CancellationToken.None);
        var seededUnit = await unitRepository.GetByCanonicalCodeAsync("ea", CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(seededUnit);
        Assert.True(seededUnit!.IsBaseUnit);
        Assert.Equal(UnitOfMeasureSeedData.Units.First(unit => unit.CanonicalCode == "ea").DisplayCode, seededUnit.DisplayCode);
        Assert.Equal(UnitOfMeasureSeedData.Units.First(unit => unit.CanonicalCode == "ea").Dimension, seededUnit.Dimension);
    }
}
