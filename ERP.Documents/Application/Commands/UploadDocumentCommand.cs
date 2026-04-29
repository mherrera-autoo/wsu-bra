namespace ERP.Documents.Application.Commands;

public sealed record UploadDocumentCommand(
    Guid CompanyPublicId,
    long UserId,
    string FileName,
    string Category,
    string ContentType,
    long SizeBytes,
    string? Description);
