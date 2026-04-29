namespace ERP.Api.Contracts.Inventory;

public sealed record TransferStockRequest(
    long CompanyId,
    long ProductId,
    long FromWarehouseId,
    long ToWarehouseId,
    decimal Quantity,
    string ReferenceType,
    string ReferenceId,
    string? Reason);
