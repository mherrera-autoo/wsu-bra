namespace ERP.Documents.Application.Queries;

public sealed record ListDocumentsQuery(
    Guid CompanyPublicId,
    string? Query,
    string? Tag,
    DateTime? CreatedFrom,
    DateTime? CreatedTo);
