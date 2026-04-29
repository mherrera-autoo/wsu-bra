namespace ERP.Documents.Contracts;

public sealed record DocumentLinked(
    long? CompanyId,
    string DocumentType,
    string DocumentId,
    string LinkedToType,
    string LinkedToId,
    string SourceModule,
    DateTime LinkedAt);
