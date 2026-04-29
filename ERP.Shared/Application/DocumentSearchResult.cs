using System.Collections.Generic;

namespace ERP.Shared.Application;

public sealed record DocumentSearchResult(
    string DocumentType,
    string DocumentId,
    float Score,
    IReadOnlyDictionary<string, string> Metadata);
