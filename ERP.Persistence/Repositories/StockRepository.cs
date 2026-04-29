using ERP.Modules.Inventory.Application.Repositories;
using ERP.Modules.Inventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class StockRepository : IStockRepository
{
    private readonly ErpDbContext _dbContext;

    public StockRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Stock?> GetAsync(long companyId, long productId, long warehouseId, CancellationToken cancellationToken = default)
        => _dbContext.Stocks
            .FirstOrDefaultAsync(s => s.CompanyId == companyId && s.ProductId == productId && s.WarehouseId == warehouseId, cancellationToken);

    public Task UpsertAsync(Stock stock, CancellationToken cancellationToken = default)
    {
        if (_dbContext.Entry(stock).State == EntityState.Detached)
        {
            if (stock.Id == 0)
            {
                _dbContext.Stocks.Add(stock);
            }
            else
            {
                _dbContext.Stocks.Update(stock);
            }
        }

        return Task.CompletedTask;
    }
}
