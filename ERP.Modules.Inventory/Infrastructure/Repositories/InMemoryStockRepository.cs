using ERP.Modules.Inventory.Application.Repositories;
using ERP.Modules.Inventory.Domain;

namespace ERP.Modules.Inventory.Infrastructure.Repositories;

public sealed class InMemoryStockRepository : IStockRepository
{
    private readonly Dictionary<(long CompanyId, long ProductId, long WarehouseId), Stock> _stocks = new();

    public Task<Stock?> GetAsync(long companyId, long productId, long warehouseId, CancellationToken cancellationToken = default)
    {
        _stocks.TryGetValue((companyId, productId, warehouseId), out var stock);
        return Task.FromResult(stock);
    }

    public Task UpsertAsync(Stock stock, CancellationToken cancellationToken = default)
    {
        if (stock is null) throw new ArgumentNullException(nameof(stock));
        _stocks[(stock.CompanyId, stock.ProductId, stock.WarehouseId)] = stock;
        return Task.CompletedTask;
    }
}
