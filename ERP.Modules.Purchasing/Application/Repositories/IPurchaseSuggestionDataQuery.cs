namespace ERP.Modules.Purchasing.Application.Repositories;

public sealed record StockSnapshot(long ProductId, long WarehouseId, decimal OnHandQuantity);

public sealed record PurchaseHistorySnapshot(long ProductId, long SupplierId, decimal OrderedQty, DateTime CreatedAt);

public interface IPurchaseSuggestionDataQuery
{
    Task<IReadOnlyList<StockSnapshot>> ListStocksAsync(long companyId, long? warehouseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PurchaseHistorySnapshot>> ListRecentPurchaseLinesAsync(
        long companyId,
        DateTime since,
        long? supplierId,
        CancellationToken cancellationToken = default);
}
