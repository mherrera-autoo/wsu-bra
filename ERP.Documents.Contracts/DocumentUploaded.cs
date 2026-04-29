namespace ERP.Documents.Contracts;

public sealed record DocumentUploaded(
    long? CompanyId,
    string DocumentType,
    string DocumentId,
    string SourceModule,
    DateTime UploadedAt,
    string? UploadedBy = null,
    string? ContextDataJson = null);
