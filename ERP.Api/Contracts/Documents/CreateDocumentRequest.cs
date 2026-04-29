using Microsoft.AspNetCore.Http;

namespace ERP.Api.Contracts.Documents;

public sealed class CreateDocumentRequest
{
    public required IFormFile File { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? Tags { get; init; }
    public string? VersionLabel { get; init; }
}
