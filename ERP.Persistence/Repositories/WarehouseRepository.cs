using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class WarehouseRepository : IWarehouseRepository
{
    private readonly ErpDbContext _dbContext;

    public WarehouseRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken = default)
    {
        await _dbContext.Warehouses.AddAsync(warehouse, cancellationToken);
    }

    public Task<bool> ExistsByCodeAsync(long companyId, string code, long? excludeId = null, CancellationToken cancellationToken = default)
        => _dbContext.Warehouses.AnyAsync(w =>
            w.CompanyId == companyId
            && w.Code == code
            && (!excludeId.HasValue || w.Id != excludeId.Value),
            cancellationToken);

    public Task<Warehouse?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => _dbContext.Warehouses.FirstOrDefaultAsync(w => w.CompanyId == companyId && w.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Warehouse>> ListAsync(long companyId, CancellationToken cancellationToken = default)
        => await _dbContext.Warehouses.AsNoTracking()
            .Where(w => w.CompanyId == companyId)
            .OrderBy(w => w.Code)
            .ToListAsync(cancellationToken);

    public void Remove(Warehouse warehouse)
    {
        _dbContext.Warehouses.Remove(warehouse);
    }
}
