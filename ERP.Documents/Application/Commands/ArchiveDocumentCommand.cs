namespace ERP.Documents.Application.Commands;

public sealed record ArchiveDocumentCommand(
    Guid CompanyPublicId,
    long DocumentId,
    long UserId,
    string? Reason);
