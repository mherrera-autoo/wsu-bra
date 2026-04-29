using ERP.Modules.MasterData.Application.Services;
using ERP.Modules.MasterData.Domain;
using Xunit;

namespace ERP.Modules.MasterData.WhiteBox.Tests;

public sealed class CompanyUnitOfMeasureServiceTests
{
    [Fact]
    public async Task DisableAsync_ShouldReject_WhenInUse()
    {
        var unitRepository = new InMemoryUnitOfMeasureRepository();
        var companyUnitRepository = new InMemoryCompanyUnitOfMeasureRepository();
        var productRepository = new InMemoryProductRepository();
        var unitOfWork = new InMemoryUnitOfWork();
        var catalog = new UnitOfMeasureCatalogService(unitRepository, unitOfWork);
        var service = new CompanyUnitOfMeasureService(companyUnitRepository, productRepository, unitRepository, unitOfWork);

        var unit = await catalog.CreateAsync("ea", "EA", UnitOfMeasureDimension.Count, true, 1m, 0, true, null);
        _ = await service.EnableAsync(1, unit.Value!.Id, null, null);
        await productRepository.AddAsync(ERP.Modules.MasterData.Domain.Product.Create(1, "SKU-1", "Test", "780000200001", unit.Value!.Id, true, true, true));

        var result = await service.DisableAsync(1, unit.Value!.Id);

        Assert.False(result.Success);
        Assert.Contains("in use", result.Error);
    }

    [Fact]
    public async Task ReplaceEnabledAsync_ShouldEnableAndDisable()
    {
        var unitRepository = new InMemoryUnitOfMeasureRepository();
        var companyUnitRepository = new InMemoryCompanyUnitOfMeasureRepository();
        var productRepository = new InMemoryProductRepository();
        var unitOfWork = new InMemoryUnitOfWork();
        var catalog = new UnitOfMeasureCatalogService(unitRepository, unitOfWork);
        var service = new CompanyUnitOfMeasureService(companyUnitRepository, productRepository, unitRepository, unitOfWork);

        var unitA = await catalog.CreateAsync("ea", "EA", UnitOfMeasureDimension.Count, true, 1m, 0, true, null);
        var unitB = await catalog.CreateAsync("kg", "KG", UnitOfMeasureDimension.Mass, true, 1m, 3, true, null);
        _ = await service.EnableAsync(1, unitA.Value!.Id, null, null);

        var replace = await service.ReplaceEnabledAsync(1, new[] { unitB.Value!.Id });

        Assert.True(replace.Success);
        Assert.True(await companyUnitRepository.IsEnabledAsync(1, unitB.Value!.Id));
        Assert.False(await companyUnitRepository.IsEnabledAsync(1, unitA.Value!.Id));
    }
}
