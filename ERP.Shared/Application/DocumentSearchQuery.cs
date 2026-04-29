using System.Collections.Generic;

namespace ERP.Shared.Application;

public sealed record DocumentSearchQuery(
    long CompanyId,
    string? Text,
    IReadOnlyDictionary<string, string>? Metadata,
    IReadOnlyList<string>? DocumentTypes,
    int Limit = 50);
