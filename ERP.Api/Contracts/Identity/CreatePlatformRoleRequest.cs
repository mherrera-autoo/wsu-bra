namespace ERP.Api.Contracts.Identity;

public sealed record CreatePlatformRoleRequest(
    string Name,
    string? Description,
    IReadOnlyCollection<long>? PermissionIds);
