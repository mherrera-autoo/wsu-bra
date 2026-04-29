using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class StockBatchRepository : IStockBatchRepository
{
    private readonly ErpDbContext _dbContext;

    public StockBatchRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<StockBatch?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.StockBatches.FirstOrDefaultAsync(batch => batch.Id == id, cancellationToken);

    public async Task<StockBatch?> GetByBatchAsync(
        long companyId,
        long productId,
        long warehouseId,
        string batchNumber,
        DateTime? expiryDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.StockBatches.FirstOrDefaultAsync(batch =>
            batch.CompanyId == companyId &&
            batch.ProductId == productId &&
            batch.WarehouseId == warehouseId &&
            batch.BatchNumber == batchNumber &&
            batch.ExpiryDate == expiryDate,
            cancellationToken);
    }

    public async Task<IReadOnlyList<StockBatch>> ListAvailableAsync(long companyId, long productId, long warehouseId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StockBatches
            .Where(batch =>
                batch.CompanyId == companyId &&
                batch.ProductId == productId &&
                batch.WarehouseId == warehouseId &&
                batch.QuantityOnHand > 0 &&
                batch.HealthStatus == StockBatchHealthStatus.Available)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StockBatch>> ListAsync(
        long companyId,
        long? productId,
        long? warehouseId,
        StockBatchHealthStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.StockBatches.AsNoTracking()
            .Where(batch => batch.CompanyId == companyId);

        if (productId.HasValue)
        {
            query = query.Where(batch => batch.ProductId == productId.Value);
        }

        if (warehouseId.HasValue)
        {
            query = query.Where(batch => batch.WarehouseId == warehouseId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(batch => batch.HealthStatus == status.Value);
        }

        return await query
            .OrderBy(batch => batch.ExpiryDate)
            .ThenBy(batch => batch.BatchNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(StockBatch stockBatch, CancellationToken cancellationToken = default)
    {
        await _dbContext.StockBatches.AddAsync(stockBatch, cancellationToken);
    }

    public Task UpdateAsync(StockBatch stockBatch, CancellationToken cancellationToken = default)
    {
        _dbContext.StockBatches.Update(stockBatch);
        return Task.CompletedTask;
    }
}
