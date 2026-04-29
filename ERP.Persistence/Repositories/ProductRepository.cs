using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class ProductRepository : IProductRepository, IProductBarcodeLookup
{
    private readonly ErpDbContext _dbContext;

    public ProductRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _dbContext.Products.AddAsync(product, cancellationToken);
    }

    public Task<bool> ExistsBySkuAsync(long companyId, string sku, long? excludeId = null, CancellationToken cancellationToken = default)
        => _dbContext.Products.AnyAsync(p =>
            p.CompanyId == companyId
            && p.Sku == sku
            && (!excludeId.HasValue || p.Id != excludeId.Value),
            cancellationToken);

    public Task<bool> ExistsByBarcodeAsync(long companyId, string barcode, long? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalized = barcode.Trim();
        return _dbContext.Products.AnyAsync(p =>
            p.CompanyId == companyId
            && p.Barcode == normalized
            && (!excludeId.HasValue || p.Id != excludeId.Value),
            cancellationToken);
    }

    public Task<bool> ExistsByPublicIdAsync(long companyId, Guid productPublicId, CancellationToken cancellationToken = default)
        => _dbContext.Products.AnyAsync(
            p => p.CompanyId == companyId && p.PublicId == productPublicId,
            cancellationToken);

    public Task<bool> ExistsByUnitOfMeasureAsync(long companyId, long unitOfMeasureId, CancellationToken cancellationToken = default)
        => _dbContext.Products.AnyAsync(
            p => p.CompanyId == companyId && p.UnitOfMeasureId == unitOfMeasureId,
            cancellationToken);

    public Task<Product?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => _dbContext.Products
            .Include(p => p.UnitOfMeasure)
            .ThenInclude(unitOfMeasure => unitOfMeasure!.Translations)
            .FirstOrDefaultAsync(p => p.CompanyId == companyId && p.Id == id, cancellationToken);

    public Task<Product?> GetByPublicIdAsync(long companyId, Guid productPublicId, CancellationToken cancellationToken = default)
        => _dbContext.Products
            .Include(p => p.UnitOfMeasure)
            .ThenInclude(unitOfMeasure => unitOfMeasure!.Translations)
            .FirstOrDefaultAsync(p => p.CompanyId == companyId && p.PublicId == productPublicId, cancellationToken);

    public Task<Product?> GetBySkuAsync(long companyId, string sku, CancellationToken cancellationToken = default)
    {
        var normalized = sku.Trim();
        return _dbContext.Products
            .Include(p => p.UnitOfMeasure)
            .ThenInclude(unitOfMeasure => unitOfMeasure!.Translations)
            .FirstOrDefaultAsync(p => p.CompanyId == companyId && p.Sku == normalized, cancellationToken);
    }

    public Task<Product?> GetByBarcodeAsync(long companyId, string barcode, CancellationToken cancellationToken = default)
    {
        var normalized = barcode.Trim();
        return _dbContext.Products
            .Include(p => p.UnitOfMeasure)
            .ThenInclude(unitOfMeasure => unitOfMeasure!.Translations)
            .FirstOrDefaultAsync(p => p.CompanyId == companyId && p.Barcode == normalized, cancellationToken);
    }

    public async Task<string?> GetBarcodeByProductIdAsync(long companyId, long productId, CancellationToken cancellationToken = default)
        => await _dbContext.Products
            .Where(p => p.CompanyId == companyId && p.Id == productId)
            .Select(p => p.Barcode)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<ProductBarcodeMatch?> GetMatchByBarcodeAsync(long companyId, string barcode, CancellationToken cancellationToken = default)
    {
        var normalized = barcode.Trim();
        return await _dbContext.Products
            .Where(p => p.CompanyId == companyId && p.Barcode == normalized)
            .Select(p => new ProductBarcodeMatch(p.Id, p.Barcode!))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> ListAsync(long companyId, CancellationToken cancellationToken = default)
        => await _dbContext.Products.AsNoTracking()
            .Include(p => p.UnitOfMeasure)
            .ThenInclude(unitOfMeasure => unitOfMeasure!.Translations)
            .Where(p => p.CompanyId == companyId)
            .OrderBy(p => p.Sku)
            .ToListAsync(cancellationToken);

    public void Remove(Product product)
    {
        _dbContext.Products.Remove(product);
    }
}
