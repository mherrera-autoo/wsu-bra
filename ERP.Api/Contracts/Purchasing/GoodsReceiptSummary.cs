namespace ERP.Api.Contracts.Purchasing;

public sealed record GoodsReceiptSummary(
    long Id,
    long SupplierId,
    long? PurchaseOrderId,
    DateTime ReceivedAt);
