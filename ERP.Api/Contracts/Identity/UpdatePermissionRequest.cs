namespace ERP.Api.Contracts.Identity;

public sealed record UpdatePermissionRequest(string Code, string Name, string? Description);
