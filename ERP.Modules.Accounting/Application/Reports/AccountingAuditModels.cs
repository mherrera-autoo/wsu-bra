namespace ERP.Modules.Accounting.Application.Reports;

public sealed record AccountingAuditLogEntry(
    DateTime OccurredAt,
    string Action,
    string EntityName,
    string EntityId,
    string ChangesJson,
    string Source,
    string? UserId);
