namespace ERP.Api.Contracts.MasterData;

public sealed record UpdateProductRequest(
    long CompanyId,
    string Sku,
    string Name,
    bool IsStockable = true,
    bool? IsStackable = null,
    decimal? LengthCm = null,
    decimal? WidthCm = null,
    decimal? WeightKg = null,
    string? StorageType = null);
