namespace ERP.Api.Contracts.MasterData;

public sealed record CreateWarehouseRequest(string Code, string Name, string? Location);
