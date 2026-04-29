namespace ERP.Api.Contracts.PharmaceuticalRegulatedInventory;

public sealed record UpsertExpirationAlertRuleRequest(
    long CompanyId,
    int DaysBeforeExpiry,
    bool IsActive);

public sealed record UpsertStockoutThresholdRequest(
    long CompanyId,
    long ProductId,
    long? WarehouseId,
    decimal ThresholdQuantity);
