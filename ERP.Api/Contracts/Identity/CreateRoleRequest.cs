namespace ERP.Api.Contracts.Identity;

public sealed record CreateRoleRequest(
    Guid CompanyPublicId,
    string Name,
    string? Description,
    IReadOnlyCollection<long>? PermissionIds);
