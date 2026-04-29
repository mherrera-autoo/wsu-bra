using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;

namespace ERP.Api.Contracts.PharmaceuticalRegulatedInventory;

public sealed record BatchAssignmentRequest(
    long CompanyId,
    long StockBatchId,
    long? WarehouseLocationId);

public sealed record BatchHoldRequest(
    long CompanyId,
    long StockBatchId,
    StockBatchHealthStatus Status,
    string Reason);

public sealed record BatchReleaseRequest(
    long CompanyId,
    long StockBatchId);

public sealed record PurchaseInvoiceReceiptRequest(
    long CompanyId,
    long SupplierId,
    long? PurchaseOrderId,
    string InvoiceNumber,
    DateTime IssuedAt,
    DateTime? DueAt,
    decimal NetAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    string Currency,
    string? QrPayload,
    IReadOnlyList<PurchaseInvoiceReceiptLineRequest> Lines);

public sealed record PurchaseInvoiceReceiptLineRequest(
    long ProductId,
    long WarehouseId,
    decimal Quantity,
    decimal UnitPrice,
    string? BatchNumber,
    DateTime? ExpiryDate,
    InboundPresentation InboundPresentation);

public sealed record PharmaceuticalRegulatedInventoryBatchResponse(
    long StockBatchId,
    long ProductId,
    long WarehouseId,
    long? WarehouseLocationId,
    string BatchNumber,
    DateTime? ExpiryDate,
    decimal QuantityOnHand,
    StockBatchHealthStatus HealthStatus);
