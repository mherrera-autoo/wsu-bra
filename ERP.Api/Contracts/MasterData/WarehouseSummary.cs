namespace ERP.Api.Contracts.MasterData;

public sealed record WarehouseSummary(
    long Id,
    Guid WarehousePublicId,
    string Code,
    string Name,
    string? Location);
