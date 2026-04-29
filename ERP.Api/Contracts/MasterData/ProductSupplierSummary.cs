namespace ERP.Api.Contracts.MasterData;

public sealed record ProductSupplierSummary(
    long Id,
    long ProductId,
    string? ProductSku,
    string? ProductName,
    long SupplierId,
    string? SupplierName,
    string? SupplierSku,
    decimal UnitPurchasePrice,
    decimal MinimumPurchaseLot,
    Guid PublicId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
