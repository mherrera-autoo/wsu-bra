using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class BlockHoldRepository : IBlockHoldRepository
{
    private readonly ErpDbContext _dbContext;

    public BlockHoldRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<BlockHold?> GetActiveByBatchAsync(long companyId, long stockBatchId, CancellationToken cancellationToken = default)
        => _dbContext.BlockHolds.FirstOrDefaultAsync(hold =>
            hold.CompanyId == companyId &&
            hold.StockBatchId == stockBatchId &&
            hold.ReleasedAt == null,
            cancellationToken);

    public async Task AddAsync(BlockHold blockHold, CancellationToken cancellationToken = default)
        => await _dbContext.BlockHolds.AddAsync(blockHold, cancellationToken);

    public Task UpdateAsync(BlockHold blockHold, CancellationToken cancellationToken = default)
    {
        _dbContext.BlockHolds.Update(blockHold);
        return Task.CompletedTask;
    }
}
