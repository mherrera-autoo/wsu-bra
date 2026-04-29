using ERP.Modules.Wms.Application.Repositories;
using ERP.Modules.Wms.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class WarehouseLocationRepository : IWarehouseLocationRepository
{
    private readonly ErpDbContext _dbContext;

    public WarehouseLocationRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(WarehouseLocation location, CancellationToken cancellationToken = default)
        => await _dbContext.WarehouseLocations.AddAsync(location, cancellationToken);

    public Task UpdateAsync(WarehouseLocation location, CancellationToken cancellationToken = default)
    {
        _dbContext.WarehouseLocations.Update(location);
        return Task.CompletedTask;
    }

    public Task<WarehouseLocation?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => _dbContext.WarehouseLocations.FirstOrDefaultAsync(location => location.CompanyId == companyId && location.Id == id, cancellationToken);

    public async Task<IReadOnlyList<WarehouseLocation>> ListByWarehouseAsync(long companyId, long warehouseId, CancellationToken cancellationToken = default)
        => await _dbContext.WarehouseLocations
            .AsNoTracking()
            .Where(location => location.CompanyId == companyId && location.WarehouseId == warehouseId)
            .OrderBy(location => location.Aisle)
            .ThenBy(location => location.Rack)
            .ThenBy(location => location.Side)
            .ThenBy(location => location.Level)
            .ThenBy(location => location.Slot)
            .ToListAsync(cancellationToken);
}
