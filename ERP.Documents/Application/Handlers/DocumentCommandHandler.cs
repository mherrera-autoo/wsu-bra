using ERP.Documents.Application.Commands;
using ERP.Documents.Application.Repositories;
using ERP.Documents.Domain;
using ERP.Shared.Application;

namespace ERP.Documents.Application.Handlers;

public sealed class DocumentCommandHandler
{
    private readonly IDocumentRepository _repository;

    public DocumentCommandHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Document>> HandleAsync(CreateDocumentCommand command, CancellationToken cancellationToken = default)
    {
        var title = string.IsNullOrWhiteSpace(command.Title)
            ? Path.GetFileNameWithoutExtension(command.FileName)
            : command.Title.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            return Result<Document>.Fail("Document title is required.");
        }

        var tags = command.Tags
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var document = Document.Create(
            command.CompanyPublicId,
            title,
            command.Description?.Trim(),
            tags,
            command.FileName,
            command.ContentType,
            command.Size,
            command.VersionLabel);

        await _repository.AddAsync(document, cancellationToken);
        var initialVersion = document.Versions.Last();
        initialVersion.AssignId(_repository.NextVersionId());
        return Result<Document>.Ok(document);
    }

    public async Task<Result<DocumentVersion>> HandleAsync(AddDocumentVersionCommand command, CancellationToken cancellationToken = default)
    {
        var document = await _repository.GetAsync(command.CompanyPublicId, command.DocumentId, cancellationToken);
        if (document is null)
        {
            return Result<DocumentVersion>.Fail("Document not found.");
        }

        var version = document.AddVersion(command.FileName, command.ContentType, command.Size, command.Label, DateTime.UtcNow);
        version.AssignId(_repository.NextVersionId());
        await _repository.UpdateAsync(document, cancellationToken);
        return Result<DocumentVersion>.Ok(version);
    }

    public async Task<Result<DocumentLink>> HandleAsync(AddDocumentLinkCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Url))
        {
            return Result<DocumentLink>.Fail("Link url is required.");
        }

        var document = await _repository.GetAsync(command.CompanyPublicId, command.DocumentId, cancellationToken);
        if (document is null)
        {
            return Result<DocumentLink>.Fail("Document not found.");
        }

        var link = document.AddLink(command.Url.Trim(), command.Description?.Trim(), DateTime.UtcNow);
        link.AssignId(_repository.NextLinkId());
        await _repository.UpdateAsync(document, cancellationToken);
        return Result<DocumentLink>.Ok(link);
    }
}
