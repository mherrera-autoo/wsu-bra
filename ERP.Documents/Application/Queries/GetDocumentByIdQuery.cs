namespace ERP.Documents.Application.Queries;

public sealed record GetDocumentByIdQuery(Guid CompanyPublicId, long DocumentId);
