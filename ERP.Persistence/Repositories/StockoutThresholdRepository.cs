using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class StockoutThresholdRepository : IStockoutThresholdRepository
{
    private readonly ErpDbContext _dbContext;

    public StockoutThresholdRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<StockoutThreshold?> GetAsync(long companyId, long productId, long? warehouseId, CancellationToken cancellationToken = default)
        => _dbContext.StockoutThresholds.FirstOrDefaultAsync(threshold =>
            threshold.CompanyId == companyId &&
            threshold.ProductId == productId &&
            threshold.WarehouseId == warehouseId,
            cancellationToken);

    public async Task AddAsync(StockoutThreshold threshold, CancellationToken cancellationToken = default)
        => await _dbContext.StockoutThresholds.AddAsync(threshold, cancellationToken);

    public Task UpdateAsync(StockoutThreshold threshold, CancellationToken cancellationToken = default)
    {
        _dbContext.StockoutThresholds.Update(threshold);
        return Task.CompletedTask;
    }
}
