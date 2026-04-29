namespace ERP.Api.Contracts.Accounting;

public sealed record AccountingAuditQuery(DateTime? From, DateTime? To, string? EntityName);
