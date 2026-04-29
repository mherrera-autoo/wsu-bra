namespace ERP.Api.Contracts.Purchasing;

public sealed record CreateGoodsReceiptRequest(
    long CompanyId,
    long SupplierId,
    long? PurchaseOrderId,
    IReadOnlyList<CreateGoodsReceiptLineRequest> Lines);

public sealed record CreateGoodsReceiptLineRequest(
    long ProductId,
    long WarehouseId,
    decimal ReceivedQty,
    string? BatchNumber,
    DateTime? ExpiryDate);
