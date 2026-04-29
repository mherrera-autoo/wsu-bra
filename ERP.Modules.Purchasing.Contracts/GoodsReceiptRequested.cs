using System.Collections.Generic;

namespace ERP.Modules.Purchasing.Contracts;

public sealed record GoodsReceiptRequested(
    long CompanyId,
    long ReceiptId,
    long SupplierId,
    long? PurchaseOrderId,
    IReadOnlyList<GoodsReceiptLine> Lines);

public sealed record GoodsReceiptLine(
    long ProductId,
    long WarehouseId,
    decimal ReceivedQty,
    string? BatchNumber,
    System.DateTime? ExpiryDate);
