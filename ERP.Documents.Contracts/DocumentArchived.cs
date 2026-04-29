namespace ERP.Documents.Contracts;

public sealed record DocumentArchived(
    long? CompanyId,
    string DocumentType,
    string DocumentId,
    string SourceModule,
    DateTime ArchivedAt,
    string? Reason = null);
