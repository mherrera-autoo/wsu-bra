using ERP.Modules.MasterData.Application.Services;
using ERP.Modules.MasterData.Domain;
using Xunit;

namespace ERP.Modules.MasterData.WhiteBox.Tests;

public sealed class MasterDataServiceTests
{
    [Fact]
    public async Task CreateProductAsync_ShouldRejectDuplicateSku()
    {
        var (service, companyUnits, catalog) = BuildService();
        var unitId = await SeedUnitAsync(companyUnits, catalog, 1, "ea", "EA");
        var first = await service.CreateProductAsync(1, "SKU-1", "Test", "780000100001", unitId, true, true, true);

        var second = await service.CreateProductAsync(1, "SKU-1", "Test 2", "780000100002", unitId, true, true, true);

        Assert.True(first.Success);
        Assert.False(second.Success);
        Assert.Contains("SKU", second.Error);
    }

    [Fact]
    public async Task CreateWarehouseAsync_ShouldRejectDuplicateCode()
    {
        var (service, _, _) = BuildService();
        var first = await service.CreateWarehouseAsync(1, "WH-1", "Main");

        var second = await service.CreateWarehouseAsync(1, "WH-1", "Secondary");

        Assert.True(first.Success);
        Assert.False(second.Success);
        Assert.Contains("Warehouse", second.Error);
    }

    [Fact]
    public async Task CreateCustomerAsync_ShouldRejectDuplicateName()
    {
        var (service, _, _) = BuildService();
        var first = await service.CreateCustomerAsync(1, "Customer A", "123");

        var second = await service.CreateCustomerAsync(1, "Customer A", "456");

        Assert.True(first.Success);
        Assert.False(second.Success);
        Assert.Contains("Customer", second.Error);
    }

    [Fact]
    public async Task CreateSupplierAsync_ShouldRejectDuplicateName()
    {
        var (service, _, _) = BuildService();
        var first = await service.CreateSupplierAsync(1, "Supplier A", "987", "CL", "CLP");

        var second = await service.CreateSupplierAsync(1, "Supplier A", "654", "CL", "CLP");

        Assert.True(first.Success);
        Assert.False(second.Success);
        Assert.Contains("Supplier", second.Error);
    }

    private static (MasterDataService Service, CompanyUnitOfMeasureService CompanyUnits, UnitOfMeasureCatalogService Catalog) BuildService()
    {
        var unitRepository = new InMemoryUnitOfMeasureRepository();
        var companyUnitRepository = new InMemoryCompanyUnitOfMeasureRepository();
        var productRepository = new InMemoryProductRepository();
        var validator = new UnitOfMeasureAccessValidator(companyUnitRepository, unitRepository);
        var unitOfWork = new InMemoryUnitOfWork();

        var service = new MasterDataService(
            new InMemoryCustomerRepository(),
            productRepository,
            new InMemorySupplierRepository(),
            validator,
            new InMemoryWarehouseRepository(),
            unitOfWork);

        var companyUnits = new CompanyUnitOfMeasureService(
            companyUnitRepository,
            productRepository,
            unitRepository,
            unitOfWork);

        var catalog = new UnitOfMeasureCatalogService(unitRepository, unitOfWork);

        return (service, companyUnits, catalog);
    }

    private static async Task<long> SeedUnitAsync(
        CompanyUnitOfMeasureService companyUnits,
        UnitOfMeasureCatalogService catalog,
        long companyId,
        string canonicalCode,
        string displayCode)
    {
        var created = await catalog.CreateAsync(
            canonicalCode,
            displayCode,
            UnitOfMeasureDimension.Count,
            true,
            1m,
            0,
            true,
            null);
        var unitId = created.Value!.Id;
        _ = await companyUnits.EnableAsync(companyId, unitId, null, null);
        return unitId;
    }
}
