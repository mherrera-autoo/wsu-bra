namespace ERP.Documents.Application.Queries;

public sealed record GetDocumentQuery(Guid CompanyPublicId, long DocumentId, long UserId);
