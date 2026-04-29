namespace ERP.Api.Contracts.Inventory;

public sealed record AdjustStockRequest(
    long CompanyId,
    long ProductId,
    long WarehouseId,
    decimal Quantity,
    bool Increase,
    string ReferenceType,
    string ReferenceId,
    string? Reason);
