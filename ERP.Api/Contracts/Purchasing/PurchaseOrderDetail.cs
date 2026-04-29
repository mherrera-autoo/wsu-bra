namespace ERP.Api.Contracts.Purchasing;

public sealed record PurchaseOrderLineDetail(long ProductId, decimal OrderedQty, decimal UnitPriceAmount, string UnitPriceCurrency);

public sealed record PurchaseOrderDetail(
    long Id,
    long SupplierId,
    string Currency,
    string Status,
    DateTime CreatedAt,
    IReadOnlyList<PurchaseOrderLineDetail> Lines);
