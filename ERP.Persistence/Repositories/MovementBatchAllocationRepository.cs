using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class MovementBatchAllocationRepository : IMovementBatchAllocationRepository
{
    private readonly ErpDbContext _dbContext;

    public MovementBatchAllocationRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MovementBatchAllocation>> ListByMovementAsync(long movementId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.MovementBatchAllocations
            .Include(allocation => allocation.StockBatch)
            .Where(allocation => allocation.MovementId == movementId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(MovementBatchAllocation allocation, CancellationToken cancellationToken = default)
    {
        await _dbContext.MovementBatchAllocations.AddAsync(allocation, cancellationToken);
    }
}
