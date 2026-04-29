using ERP.Modules.Wms.Application.Repositories;
using ERP.Modules.Wms.Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ERP.Persistence.Repositories;

public sealed class WarehouseOperationRepository : IWarehouseOperationRepository
{
    private readonly ErpDbContext _dbContext;

    public WarehouseOperationRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(WarehouseOperation operation, CancellationToken cancellationToken = default)
    {
        await _dbContext.WarehouseOperations.AddAsync(operation, cancellationToken);
    }

    public Task UpdateAsync(WarehouseOperation operation, CancellationToken cancellationToken = default)
    {
        _dbContext.WarehouseOperations.Update(operation);
        return Task.CompletedTask;
    }

    public Task<WarehouseOperation?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => _dbContext.WarehouseOperations.FirstOrDefaultAsync(op => op.CompanyId == companyId && op.Id == id, cancellationToken);

    public async Task<IReadOnlyList<WarehouseOperation>> ListByWarehouseAsync(long companyId, long warehouseId, CancellationToken cancellationToken = default)
        => await _dbContext.WarehouseOperations.AsNoTracking()
            .Where(op => op.CompanyId == companyId && op.WarehouseId == warehouseId)
            .OrderByDescending(op => op.CreatedAt)
            .ToListAsync(cancellationToken);
}
