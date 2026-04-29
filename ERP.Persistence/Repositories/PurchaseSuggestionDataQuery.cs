using ERP.Modules.Purchasing.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class PurchaseSuggestionDataQuery : IPurchaseSuggestionDataQuery
{
    private readonly ErpDbContext _dbContext;

    public PurchaseSuggestionDataQuery(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<StockSnapshot>> ListStocksAsync(long companyId, long? warehouseId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Stocks.AsNoTracking()
            .Where(stock => stock.CompanyId == companyId);

        if (warehouseId.HasValue)
        {
            query = query.Where(stock => stock.WarehouseId == warehouseId.Value);
        }

        return await query
            .Select(stock => new StockSnapshot(stock.ProductId, stock.WarehouseId, stock.OnHandQuantity))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PurchaseHistorySnapshot>> ListRecentPurchaseLinesAsync(
        long companyId,
        DateTime since,
        long? supplierId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.PurchaseOrderLines.AsNoTracking()
            .Join(
                _dbContext.PurchaseOrders.AsNoTracking(),
                line => line.PurchaseOrderId,
                order => order.Id,
                (line, order) => new { line, order })
            .Where(entry => entry.order.CompanyId == companyId && entry.order.CreatedAt >= since);

        if (supplierId.HasValue)
        {
            query = query.Where(entry => entry.order.SupplierId == supplierId.Value);
        }

        return await query
            .OrderByDescending(entry => entry.order.CreatedAt)
            .Select(entry => new PurchaseHistorySnapshot(
                entry.line.ProductId,
                entry.order.SupplierId,
                entry.line.OrderedQty,
                entry.order.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
