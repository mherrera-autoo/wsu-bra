using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.WhiteBox.Tests;

internal sealed class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products = new();

    public Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        _products.Add(product);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsBySkuAsync(long companyId, string sku, long? excludeId = null, CancellationToken cancellationToken = default)
        => Task.FromResult(_products.Any(p =>
            p.CompanyId == companyId
            && p.Sku == sku
            && (!excludeId.HasValue || p.Id != excludeId.Value)));

    public Task<bool> ExistsByBarcodeAsync(long companyId, string barcode, long? excludeId = null, CancellationToken cancellationToken = default)
        => Task.FromResult(_products.Any(p =>
            p.CompanyId == companyId
            && p.Barcode == barcode.Trim()
            && (!excludeId.HasValue || p.Id != excludeId.Value)));

    public Task<bool> ExistsByPublicIdAsync(long companyId, Guid productPublicId, CancellationToken cancellationToken = default)
        => Task.FromResult(_products.Any(p =>
            p.CompanyId == companyId
            && p.PublicId == productPublicId));

    public Task<bool> ExistsByUnitOfMeasureAsync(long companyId, long unitOfMeasureId, CancellationToken cancellationToken = default)
        => Task.FromResult(_products.Any(p =>
            p.CompanyId == companyId
            && p.UnitOfMeasureId == unitOfMeasureId));

    public Task<Product?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => Task.FromResult(_products.FirstOrDefault(p => p.CompanyId == companyId && p.Id == id));

    public Task<Product?> GetByPublicIdAsync(long companyId, Guid productPublicId, CancellationToken cancellationToken = default)
        => Task.FromResult(_products.FirstOrDefault(p => p.CompanyId == companyId && p.PublicId == productPublicId));

    public Task<Product?> GetBySkuAsync(long companyId, string sku, CancellationToken cancellationToken = default)
        => Task.FromResult(_products.FirstOrDefault(p => p.CompanyId == companyId && p.Sku == sku.Trim()));

    public Task<Product?> GetByBarcodeAsync(long companyId, string barcode, CancellationToken cancellationToken = default)
        => Task.FromResult(_products.FirstOrDefault(p =>
            p.CompanyId == companyId && p.Barcode == barcode.Trim()));

    public Task<IReadOnlyList<Product>> ListAsync(long companyId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Product>>(_products.Where(p => p.CompanyId == companyId).ToList());

    public void Remove(Product product)
    {
        _products.Remove(product);
    }
}

internal sealed class InMemoryWarehouseRepository : IWarehouseRepository
{
    private readonly List<Warehouse> _warehouses = new();

    public Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken = default)
    {
        _warehouses.Add(warehouse);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByCodeAsync(long companyId, string code, long? excludeId = null, CancellationToken cancellationToken = default)
        => Task.FromResult(_warehouses.Any(w =>
            w.CompanyId == companyId
            && w.Code == code
            && (!excludeId.HasValue || w.Id != excludeId.Value)));

    public Task<Warehouse?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => Task.FromResult(_warehouses.FirstOrDefault(w => w.CompanyId == companyId && w.Id == id));

    public Task<IReadOnlyList<Warehouse>> ListAsync(long companyId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Warehouse>>(_warehouses.Where(w => w.CompanyId == companyId).ToList());

    public void Remove(Warehouse warehouse)
    {
        _warehouses.Remove(warehouse);
    }
}

internal sealed class InMemoryCustomerRepository : ICustomerRepository
{
    private readonly List<Customer> _customers = new();

    public Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        _customers.Add(customer);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByNameAsync(long companyId, string name, long? excludeId = null, CancellationToken cancellationToken = default)
        => Task.FromResult(_customers.Any(c =>
            c.CompanyId == companyId
            && c.Name == name
            && (!excludeId.HasValue || c.Id != excludeId.Value)));

    public Task<Customer?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => Task.FromResult(_customers.FirstOrDefault(c => c.CompanyId == companyId && c.Id == id));

    public Task<IReadOnlyList<Customer>> ListAsync(long companyId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Customer>>(_customers.Where(c => c.CompanyId == companyId).ToList());

    public void Remove(Customer customer)
    {
        _customers.Remove(customer);
    }
}

internal sealed class InMemorySupplierRepository : ISupplierRepository
{
    private readonly List<Supplier> _suppliers = new();

    public Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default)
    {
        _suppliers.Add(supplier);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByNameAsync(long companyId, string name, long? excludeId = null, CancellationToken cancellationToken = default)
        => Task.FromResult(_suppliers.Any(s =>
            s.CompanyId == companyId
            && s.Name == name
            && (!excludeId.HasValue || s.Id != excludeId.Value)));

    public Task<bool> ExistsByTaxIdAsync(long companyId, string taxId, long? excludeId = null, CancellationToken cancellationToken = default)
        => Task.FromResult(_suppliers.Any(s =>
            s.CompanyId == companyId
            && s.TaxId == taxId
            && (!excludeId.HasValue || s.Id != excludeId.Value)));

    public Task<Supplier?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => Task.FromResult(_suppliers.FirstOrDefault(s => s.CompanyId == companyId && s.Id == id));

    public Task<IReadOnlyList<Supplier>> ListAsync(
        long companyId,
        string? search = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _suppliers.Where(s => s.CompanyId == companyId);

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(s =>
                s.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                || (s.TaxId?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return Task.FromResult<IReadOnlyList<Supplier>>(query.OrderBy(s => s.Name).ToList());
    }

    public void Remove(Supplier supplier)
    {
        _suppliers.Remove(supplier);
    }
}

internal sealed class InMemoryUnitOfMeasureRepository : IUnitOfMeasureRepository
{
    private readonly List<UnitOfMeasure> _units = new();

    public Task AddAsync(UnitOfMeasure unitOfMeasure, CancellationToken cancellationToken = default)
    {
        if (unitOfMeasure.Id == 0)
        {
            var nextId = _units.Count == 0 ? 1 : _units.Max(u => u.Id) + 1;
            typeof(UnitOfMeasure).GetProperty("Id")!.SetValue(unitOfMeasure, nextId);
        }

        _units.Add(unitOfMeasure);
        return Task.CompletedTask;
    }

    public Task<UnitOfMeasure?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => Task.FromResult(_units.FirstOrDefault(unit => unit.Id == id));

    public Task<UnitOfMeasure?> GetByCanonicalCodeAsync(string canonicalCode, CancellationToken cancellationToken = default)
        => Task.FromResult(_units.FirstOrDefault(unit =>
            string.Equals(unit.CanonicalCode, canonicalCode.Trim(), StringComparison.OrdinalIgnoreCase)));

    public Task<bool> ExistsByCanonicalCodeAsync(string canonicalCode, long? excludeId = null, CancellationToken cancellationToken = default)
        => Task.FromResult(_units.Any(unit =>
            string.Equals(unit.CanonicalCode, canonicalCode.Trim(), StringComparison.OrdinalIgnoreCase)
            && (!excludeId.HasValue || unit.Id != excludeId.Value)));

    public Task<IReadOnlyList<UnitOfMeasure>> ListAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<UnitOfMeasure>>(_units.ToList());
}

internal sealed class InMemoryCompanyUnitOfMeasureRepository : ICompanyUnitOfMeasureRepository
{
    private readonly List<CompanyUnitOfMeasure> _companyUnits = new();

    public Task AddAsync(CompanyUnitOfMeasure companyUnitOfMeasure, CancellationToken cancellationToken = default)
    {
        _companyUnits.Add(companyUnitOfMeasure);
        return Task.CompletedTask;
    }

    public Task<CompanyUnitOfMeasure?> GetAsync(long companyId, long unitOfMeasureId, CancellationToken cancellationToken = default)
        => Task.FromResult(_companyUnits.FirstOrDefault(item =>
            item.CompanyId == companyId && item.UnitOfMeasureId == unitOfMeasureId));

    public Task<IReadOnlyList<CompanyUnitOfMeasure>> ListByCompanyAsync(long companyId, bool enabledOnly, CancellationToken cancellationToken = default)
    {
        var query = _companyUnits.Where(item => item.CompanyId == companyId);
        if (enabledOnly)
        {
            query = query.Where(item => item.IsEnabled);
        }

        return Task.FromResult<IReadOnlyList<CompanyUnitOfMeasure>>(query.ToList());
    }

    public Task<bool> IsEnabledAsync(long companyId, long unitOfMeasureId, CancellationToken cancellationToken = default)
        => Task.FromResult(_companyUnits.Any(item =>
            item.CompanyId == companyId && item.UnitOfMeasureId == unitOfMeasureId && item.IsEnabled));

    public Task<CompanyUnitOfMeasure?> GetDefaultForDimensionAsync(long companyId, UnitOfMeasureDimension dimension, CancellationToken cancellationToken = default)
        => Task.FromResult(_companyUnits.FirstOrDefault(item =>
            item.CompanyId == companyId && item.Dimension == dimension && item.IsDefaultForDimension));
}

internal sealed class InMemoryUnitOfMeasureTranslationRepository : IUnitOfMeasureTranslationRepository
{
    private readonly List<UnitOfMeasureTranslation> _translations = new();

    public Task AddAsync(UnitOfMeasureTranslation translation, CancellationToken cancellationToken = default)
    {
        if (translation.Id == 0)
        {
            var nextId = _translations.Count == 0 ? 1 : _translations.Max(t => t.Id) + 1;
            typeof(UnitOfMeasureTranslation).GetProperty("Id")!.SetValue(translation, nextId);
        }

        _translations.Add(translation);
        return Task.CompletedTask;
    }

    public Task<UnitOfMeasureTranslation?> GetAsync(long unitOfMeasureId, string culture, CancellationToken cancellationToken = default)
        => Task.FromResult(_translations.FirstOrDefault(t =>
            t.UnitOfMeasureId == unitOfMeasureId
            && string.Equals(t.Culture, culture, StringComparison.OrdinalIgnoreCase)));
}

internal sealed class InMemoryUnitOfMeasureExternalMappingRepository : IUnitOfMeasureExternalMappingRepository
{
    private readonly List<UnitOfMeasureExternalMapping> _mappings = new();

    public Task AddAsync(UnitOfMeasureExternalMapping mapping, CancellationToken cancellationToken = default)
    {
        if (mapping.Id == 0)
        {
            var nextId = _mappings.Count == 0 ? 1 : _mappings.Max(m => m.Id) + 1;
            typeof(UnitOfMeasureExternalMapping).GetProperty("Id")!.SetValue(mapping, nextId);
        }

        _mappings.Add(mapping);
        return Task.CompletedTask;
    }

    public Task<UnitOfMeasureExternalMapping?> GetAsync(UnitOfMeasureMappingScheme scheme, string code, CancellationToken cancellationToken = default)
        => Task.FromResult(_mappings.FirstOrDefault(m =>
            m.Scheme == scheme
            && string.Equals(m.Code, code, StringComparison.OrdinalIgnoreCase)));
}

internal sealed class InMemoryProductUnitConversionRepository : IProductUnitConversionRepository
{
    private readonly List<ProductUnitConversion> _conversions = new();

    public Task AddAsync(ProductUnitConversion conversion, CancellationToken cancellationToken = default)
    {
        if (conversion.Id == 0)
        {
            var nextId = _conversions.Count == 0 ? 1 : _conversions.Max(c => c.Id) + 1;
            typeof(ProductUnitConversion).GetProperty("Id")!.SetValue(conversion, nextId);
        }

        _conversions.Add(conversion);
        return Task.CompletedTask;
    }

    public Task<ProductUnitConversion?> GetAsync(long productId, long fromUnitOfMeasureId, long toUnitOfMeasureId, CancellationToken cancellationToken = default)
        => Task.FromResult(_conversions.FirstOrDefault(c =>
            c.ProductId == productId
            && c.FromUnitOfMeasureId == fromUnitOfMeasureId
            && c.ToUnitOfMeasureId == toUnitOfMeasureId));
}
