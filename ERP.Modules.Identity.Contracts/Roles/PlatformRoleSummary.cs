using System;

namespace ERP.Modules.Identity.Contracts;

public sealed record PlatformRoleSummary(
    Guid PublicId,
    string Name,
    string? Description,
    bool IsActive,
    IReadOnlyCollection<PermissionSummary> Permissions,
    bool IsAssignedToUser = false);
