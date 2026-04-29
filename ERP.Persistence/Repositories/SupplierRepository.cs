using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class SupplierRepository : ISupplierRepository
{
    private readonly ErpDbContext _dbContext;

    public SupplierRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default)
    {
        await _dbContext.Suppliers.AddAsync(supplier, cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(long companyId, string name, long? excludeId = null, CancellationToken cancellationToken = default)
        => _dbContext.Suppliers.AnyAsync(s =>
            s.CompanyId == companyId
            && s.Name == name
            && (!excludeId.HasValue || s.Id != excludeId.Value),
            cancellationToken);

    public Task<bool> ExistsByTaxIdAsync(long companyId, string taxId, long? excludeId = null, CancellationToken cancellationToken = default)
        => _dbContext.Suppliers.AnyAsync(s =>
            s.CompanyId == companyId
            && s.TaxId == taxId
            && (!excludeId.HasValue || s.Id != excludeId.Value),
            cancellationToken);

    public Task<Supplier?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => _dbContext.Suppliers.FirstOrDefaultAsync(s => s.CompanyId == companyId && s.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Supplier>> ListAsync(
        long companyId,
        string? search = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Suppliers.AsNoTracking()
            .Where(s => s.CompanyId == companyId);

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(s =>
                EF.Functions.ILike(s.Name, pattern)
                || (s.TaxId != null && EF.Functions.ILike(s.TaxId, pattern)));
        }

        return await query.OrderBy(s => s.Name).ToListAsync(cancellationToken);
    }

    public void Remove(Supplier supplier)
    {
        _dbContext.Suppliers.Remove(supplier);
    }
}
