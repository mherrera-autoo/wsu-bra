using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;

namespace ERP.Api.Contracts.PharmaceuticalRegulatedInventory;

public sealed record ExpiringStockReportQuery(
    DateTime? From,
    DateTime? To,
    int? DaysThreshold,
    long? ProductId,
    long? WarehouseId,
    StockBatchHealthStatus? HealthStatus);

public sealed record ControlledSubstanceReportQuery(
    DateTime? From,
    DateTime? To,
    long? ProductId,
    ControlledMovementType? MovementType,
    string? BatchNumber);

public sealed record WasteReportQuery(
    DateTime? From,
    DateTime? To,
    long? ProductId,
    long? WarehouseId);

public sealed record InventoryDifferenceReportQuery(
    DateTime? From,
    DateTime? To,
    long? ProductId,
    long? WarehouseId,
    string? ReferenceType);

public sealed record StockoutReportQuery(
    long? ProductId,
    long? WarehouseId);
