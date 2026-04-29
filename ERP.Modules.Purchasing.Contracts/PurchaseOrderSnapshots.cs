using ERP.Shared.Domain.ValueObjects;
using System.Collections.Generic;

namespace ERP.Modules.Purchasing.Contracts;

public sealed record PurchaseOrderSnapshot(
    long Id,
    long CompanyId,
    long SupplierId,
    string Currency,
    IReadOnlyList<PurchaseOrderLineSnapshot> Lines);

public sealed record PurchaseOrderLineSnapshot(
    long ProductId,
    decimal OrderedQty,
    Money UnitPriceRef);

public sealed record GoodsReceiptSnapshot(
    long Id,
    long CompanyId,
    long SupplierId,
    long? PurchaseOrderId,
    IReadOnlyList<GoodsReceiptLineSnapshot> Lines);

public sealed record GoodsReceiptLineSnapshot(
    long ProductId,
    decimal ReceivedQty);
