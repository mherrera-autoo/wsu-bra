namespace ERP.Api.Contracts.Identity;

public sealed record UpdatePlatformRoleRequest(
    string Name,
    string? Description,
    bool IsActive,
    IReadOnlyCollection<long>? PermissionIds);
