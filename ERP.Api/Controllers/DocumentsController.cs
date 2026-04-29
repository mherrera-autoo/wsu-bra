using ERP.Api.Authorization;
using ERP.Api.Contracts.Documents;
using ERP.Documents.Application.Commands;
using ERP.Documents.Application.Handlers;
using ERP.Documents.Application.Queries;
using ERP.Documents.Domain;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/documents")]
public sealed class DocumentsController : ControllerBase
{
    private readonly DocumentCommandHandler _commandHandler;
    private readonly DocumentQueryHandler _queryHandler;
    private readonly ICurrentUserProvider _currentUserProvider;

    public DocumentsController(
        DocumentCommandHandler commandHandler,
        DocumentQueryHandler queryHandler,
        ICurrentUserProvider currentUserProvider)
    {
        _commandHandler = commandHandler;
        _queryHandler = queryHandler;
        _currentUserProvider = currentUserProvider;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateDocument([FromForm] CreateDocumentRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyPublicId(out var companyPublicId))
        {
            return Unauthorized();
        }

        if (request.File is null || request.File.Length == 0)
        {
            return BadRequest(new { error = "File is required." });
        }

        var tags = ParseTags(request.Tags);
        var command = new CreateDocumentCommand(
            companyPublicId,
            request.File.FileName,
            request.File.ContentType,
            request.File.Length,
            request.Title,
            request.Description,
            tags,
            request.VersionLabel);

        var result = await _commandHandler.HandleAsync(command, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        var document = result.Value!;
        return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, ToDetail(document));
    }

    [HttpPost("{id:long}/versions")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddVersion(long id, [FromForm] AddDocumentVersionRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyPublicId(out var companyPublicId))
        {
            return Unauthorized();
        }

        if (request.File is null || request.File.Length == 0)
        {
            return BadRequest(new { error = "File is required." });
        }

        var result = await _commandHandler.HandleAsync(
            new AddDocumentVersionCommand(
                companyPublicId,
                id,
                request.File.FileName,
                request.File.ContentType,
                request.File.Length,
                request.Label),
            cancellationToken);

        if (!result.Success)
        {
            if (string.Equals(result.Error, "Document not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(ToSummary(result.Value!));
    }

    [HttpPost("{id:long}/links")]
    public async Task<IActionResult> AddLink(long id, AddDocumentLinkRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyPublicId(out var companyPublicId))
        {
            return Unauthorized();
        }

        var result = await _commandHandler.HandleAsync(
            new AddDocumentLinkCommand(companyPublicId, id, request.Url, request.Description),
            cancellationToken);

        if (!result.Success)
        {
            if (string.Equals(result.Error, "Document not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(ToSummary(result.Value!));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<DocumentDetail>> GetDocument(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyPublicId(out var companyPublicId))
        {
            return Unauthorized();
        }

        var document = await _queryHandler.HandleAsync(new GetDocumentByIdQuery(companyPublicId, id), cancellationToken);
        if (document is null)
        {
            return NotFound();
        }

        return Ok(ToDetail(document));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DocumentSummary>>> ListDocuments(
        [FromQuery] string? q,
        [FromQuery] string? tag,
        [FromQuery] DateTime? createdFrom,
        [FromQuery] DateTime? createdTo,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyPublicId(out var companyPublicId))
        {
            return Unauthorized();
        }

        var documents = await _queryHandler.HandleAsync(
            new ListDocumentsQuery(companyPublicId, q, tag, createdFrom, createdTo),
            cancellationToken);

        var response = documents.Select(ToSummary).ToList();
        return Ok(response);
    }

    private bool TryGetCompanyPublicId(out Guid companyPublicId)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null)
        {
            companyPublicId = default;
            return false;
        }

        if (!currentUser.CompanyPublicId.HasValue)
        {
            companyPublicId = default;
            return false;
        }

        companyPublicId = currentUser.CompanyPublicId.Value;
        return true;
    }

    private static IReadOnlyList<string> ParseTags(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Array.Empty<string>();
        }

        return raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }

    private static DocumentSummary ToSummary(Document document)
    {
        var latestVersion = document.Versions.OrderByDescending(version => version.UploadedAt).First();
        return new DocumentSummary(
            document.Id,
            document.Title,
            document.Description,
            document.Tags.ToList(),
            document.CreatedAt,
            ToSummary(latestVersion));
    }

    private static DocumentDetail ToDetail(Document document)
        => new(
            document.Id,
            document.Title,
            document.Description,
            document.Tags.ToList(),
            document.CreatedAt,
            document.Versions.Select(ToSummary).ToList(),
            document.Links.Select(ToSummary).ToList());

    private static DocumentVersionSummary ToSummary(DocumentVersion version)
        => new(
            version.Id,
            version.FileName,
            version.ContentType,
            version.Size,
            version.Label,
            version.UploadedAt);

    private static DocumentLinkSummary ToSummary(DocumentLink link)
        => new(
            link.Id,
            link.Url,
            link.Description,
            link.CreatedAt);
}
