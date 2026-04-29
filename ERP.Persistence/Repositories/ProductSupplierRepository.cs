using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class ProductSupplierRepository : IProductSupplierRepository
{
    private readonly ErpDbContext _dbContext;

    public ProductSupplierRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ProductSupplier?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => _dbContext.ProductSuppliers
            .Include(ps => ps.Product)
            .Include(ps => ps.Supplier)
            .FirstOrDefaultAsync(ps => ps.CompanyId == companyId && ps.Id == id, cancellationToken);

    public Task<ProductSupplier?> GetByProductAndSupplierAsync(long companyId, long productId, long supplierId, CancellationToken cancellationToken = default)
        => _dbContext.ProductSuppliers
            .Include(ps => ps.Product)
            .Include(ps => ps.Supplier)
            .FirstOrDefaultAsync(
                ps => ps.CompanyId == companyId && ps.ProductId == productId && ps.SupplierId == supplierId,
                cancellationToken);

    public async Task<IReadOnlyList<ProductSupplier>> ListByProductAsync(long companyId, long productId, CancellationToken cancellationToken = default)
        => await _dbContext.ProductSuppliers
            .AsNoTracking()
            .Include(ps => ps.Supplier)
            .Where(ps => ps.CompanyId == companyId && ps.ProductId == productId)
            .OrderBy(ps => ps.Supplier.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ProductSupplier>> ListBySupplierAsync(long companyId, long supplierId, CancellationToken cancellationToken = default)
        => await _dbContext.ProductSuppliers
            .AsNoTracking()
            .Include(ps => ps.Product)
            .Where(ps => ps.CompanyId == companyId && ps.SupplierId == supplierId)
            .OrderBy(ps => ps.Product.Sku)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(ProductSupplier productSupplier, CancellationToken cancellationToken = default)
    {
        await _dbContext.ProductSuppliers.AddAsync(productSupplier, cancellationToken);
    }

    public void Remove(ProductSupplier productSupplier)
    {
        _dbContext.ProductSuppliers.Remove(productSupplier);
    }
}
