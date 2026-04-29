namespace ERP.Documents.Application.Commands;

public sealed record AddDocumentVersionCommand(
    Guid CompanyPublicId,
    long DocumentId,
    string FileName,
    string ContentType,
    long Size,
    string? Label);
