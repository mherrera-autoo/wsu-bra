namespace ERP.Api.Contracts.MasterData;

public sealed record CreateProductRequest(
    long CompanyId,
    string Sku,
    string Name,
    string? Barcode,
    long UnitOfMeasureId,
    bool IsStockable = true,
    bool IsSellable = true,
    bool IsPurchasable = true,
    bool? IsStackable = null,
    decimal? LengthCm = null,
    decimal? WidthCm = null,
    decimal? WeightKg = null,
    string? StorageType = null);
