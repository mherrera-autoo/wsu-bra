namespace ERP.Modules.Identity.Contracts;

public sealed record PermissionSummary(long Id, string Code, string Name, string? Description, bool IsActive);
