using System.Collections.Generic;

namespace ERP.Shared.Application;

public sealed record DocumentIndexRequest(
    long CompanyId,
    string DocumentType,
    string DocumentId,
    string? Title,
    string Content,
    IReadOnlyDictionary<string, string>? Metadata);
