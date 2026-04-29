namespace ERP.Api.Contracts.Identity;

public sealed record CreatePermissionRequest(string Code, string Name, string? Description);
