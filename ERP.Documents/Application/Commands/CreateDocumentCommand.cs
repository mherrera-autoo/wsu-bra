namespace ERP.Documents.Application.Commands;

public sealed record CreateDocumentCommand(
    Guid CompanyPublicId,
    string FileName,
    string ContentType,
    long Size,
    string? Title,
    string? Description,
    IReadOnlyList<string> Tags,
    string? VersionLabel);
