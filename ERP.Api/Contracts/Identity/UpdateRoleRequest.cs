namespace ERP.Api.Contracts.Identity;

public sealed record UpdateRoleRequest(
    Guid CompanyPublicId,
    string Name,
    string? Description,
    bool IsActive,
    IReadOnlyCollection<long>? PermissionIds);
