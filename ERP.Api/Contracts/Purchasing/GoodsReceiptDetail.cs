namespace ERP.Api.Contracts.Purchasing;

public sealed record GoodsReceiptLineDetail(long ProductId, long WarehouseId, decimal ReceivedQty);

public sealed record GoodsReceiptDetail(
    long Id,
    long SupplierId,
    long? PurchaseOrderId,
    DateTime ReceivedAt,
    IReadOnlyList<GoodsReceiptLineDetail> Lines);
