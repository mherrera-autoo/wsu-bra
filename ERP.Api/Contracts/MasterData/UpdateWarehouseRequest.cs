namespace ERP.Api.Contracts.MasterData;

public sealed record UpdateWarehouseRequest(string Code, string Name, bool IsActive, string? Location);
