namespace ERP.Documents.Application.Commands;

public sealed record LinkDocumentCommand(
    Guid CompanyPublicId,
    long DocumentId,
    long UserId,
    string LinkedEntityType,
    long LinkedEntityId);
