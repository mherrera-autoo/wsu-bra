namespace ERP.Api.Contracts.MasterData;

public sealed record ProductSummary(
    long Id,
    string Sku,
    string Name,
    string? Barcode,
    bool IsStockable,
    bool IsSellable,
    bool IsPurchasable,
    bool? IsStackable,
    decimal? LengthCm,
    decimal? WidthCm,
    decimal? WeightKg,
    string? StorageType,
    string UnitOfMeasureDisplayCode,
    string UnitOfMeasureSymbol);
