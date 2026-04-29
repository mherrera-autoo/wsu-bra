using Microsoft.AspNetCore.Http;

namespace ERP.Api.Contracts.Documents;

public sealed class AddDocumentVersionRequest
{
    public required IFormFile File { get; init; }
    public string? Label { get; init; }
}
