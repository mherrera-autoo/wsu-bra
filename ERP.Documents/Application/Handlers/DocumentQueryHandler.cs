using ERP.Documents.Application.Queries;
using ERP.Documents.Application.Repositories;
using ERP.Documents.Domain;

namespace ERP.Documents.Application.Handlers;

public sealed class DocumentQueryHandler
{
    private readonly IDocumentRepository _repository;

    public DocumentQueryHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public Task<Document?> HandleAsync(GetDocumentByIdQuery query, CancellationToken cancellationToken = default)
        => _repository.GetAsync(query.CompanyPublicId, query.DocumentId, cancellationToken);

    public async Task<IReadOnlyList<Document>> HandleAsync(ListDocumentsQuery query, CancellationToken cancellationToken = default)
    {
        var documents = await _repository.ListAsync(query.CompanyPublicId, cancellationToken);
        var filtered = documents.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.Query))
        {
            var term = query.Query.Trim();
            filtered = filtered.Where(document => document.Title.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Tag))
        {
            var tag = query.Tag.Trim();
            filtered = filtered.Where(document => document.Tags.Any(documentTag =>
                string.Equals(documentTag, tag, StringComparison.OrdinalIgnoreCase)));
        }

        if (query.CreatedFrom.HasValue)
        {
            filtered = filtered.Where(document => document.CreatedAt >= query.CreatedFrom.Value);
        }

        if (query.CreatedTo.HasValue)
        {
            filtered = filtered.Where(document => document.CreatedAt <= query.CreatedTo.Value);
        }

        return filtered.OrderByDescending(document => document.CreatedAt).ToList();
    }
}
