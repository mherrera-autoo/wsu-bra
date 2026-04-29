namespace ERP.Api.Contracts.Documents;

public sealed record DocumentSummary(
    long Id,
    string Title,
    string? Description,
    IReadOnlyList<string> Tags,
    DateTime CreatedAt,
    DocumentVersionSummary LatestVersion);

public sealed record DocumentDetail(
    long Id,
    string Title,
    string? Description,
    IReadOnlyList<string> Tags,
    DateTime CreatedAt,
    IReadOnlyList<DocumentVersionSummary> Versions,
    IReadOnlyList<DocumentLinkSummary> Links);

public sealed record DocumentVersionSummary(
    long Id,
    string FileName,
    string ContentType,
    long Size,
    string? Label,
    DateTime UploadedAt);

public sealed record DocumentLinkSummary(
    long Id,
    string Url,
    string? Description,
    DateTime CreatedAt);
