using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class QuarantineHoldRepository : IQuarantineHoldRepository
{
    private readonly ErpDbContext _dbContext;

    public QuarantineHoldRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<QuarantineHold?> GetActiveByBatchAsync(long companyId, long stockBatchId, CancellationToken cancellationToken = default)
        => _dbContext.QuarantineHolds.FirstOrDefaultAsync(hold =>
            hold.CompanyId == companyId &&
            hold.StockBatchId == stockBatchId &&
            hold.ReleasedAt == null,
            cancellationToken);

    public async Task AddAsync(QuarantineHold quarantineHold, CancellationToken cancellationToken = default)
    {
        await _dbContext.QuarantineHolds.AddAsync(quarantineHold, cancellationToken);
    }

    public Task UpdateAsync(QuarantineHold quarantineHold, CancellationToken cancellationToken = default)
    {
        _dbContext.QuarantineHolds.Update(quarantineHold);
        return Task.CompletedTask;
    }
}
