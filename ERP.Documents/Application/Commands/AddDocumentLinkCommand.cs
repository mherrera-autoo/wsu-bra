namespace ERP.Documents.Application.Commands;

public sealed record AddDocumentLinkCommand(
    Guid CompanyPublicId,
    long DocumentId,
    string Url,
    string? Description);
