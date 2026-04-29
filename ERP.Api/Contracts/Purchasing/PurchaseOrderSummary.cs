namespace ERP.Api.Contracts.Purchasing;

public sealed record PurchaseOrderSummary(
    long Id,
    long SupplierId,
    string Currency,
    string Status,
    DateTime CreatedAt);
