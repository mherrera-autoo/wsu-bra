using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class StockLedgerEntryRepository : IStockLedgerEntryRepository
{
    private readonly ErpDbContext _dbContext;

    public StockLedgerEntryRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(StockLedgerEntry entry, CancellationToken cancellationToken = default)
        => await _dbContext.StockLedgerEntries.AddAsync(entry, cancellationToken);

    public async Task<IReadOnlyList<StockLedgerEntry>> ListByBatchAsync(long companyId, long stockBatchId, CancellationToken cancellationToken = default)
        => await _dbContext.StockLedgerEntries
            .AsNoTracking()
            .Where(entry => entry.CompanyId == companyId && entry.StockBatchId == stockBatchId)
            .OrderByDescending(entry => entry.OccurredAt)
            .ToListAsync(cancellationToken);
}
