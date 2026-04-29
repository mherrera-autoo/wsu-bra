namespace ERP.Modules.Identity.Contracts;

public sealed record RoleSummary(
    long Id,
    Guid CompanyPublicId,
    long CompanyId,
    string Name,
    string? Description,
    bool IsActive,
    IReadOnlyCollection<PermissionSummary> Permissions,
    bool IsAssigned = false);
