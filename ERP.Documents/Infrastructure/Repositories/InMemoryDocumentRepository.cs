using ERP.Documents.Application.Repositories;
using ERP.Documents.Domain;

namespace ERP.Documents.Infrastructure.Repositories;

public sealed class InMemoryDocumentRepository : IDocumentRepository
{
    private readonly Dictionary<(Guid CompanyPublicId, long DocumentId), Document> _documents = new();
    private long _nextDocumentId = 1;
    private long _nextVersionId = 1;
    private long _nextLinkId = 1;

    public Task AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        if (document is null) throw new ArgumentNullException(nameof(document));

        document.AssignId(_nextDocumentId++);
        _documents[(document.CompanyPublicId, document.Id)] = document;
        return Task.CompletedTask;
    }

    public Task<Document?> GetAsync(Guid companyPublicId, long documentId, CancellationToken cancellationToken = default)
    {
        _documents.TryGetValue((companyPublicId, documentId), out var document);
        return Task.FromResult(document);
    }

    public Task<IReadOnlyList<Document>> ListAsync(Guid companyPublicId, CancellationToken cancellationToken = default)
    {
        var documents = _documents
            .Where(item => item.Key.CompanyPublicId == companyPublicId)
            .Select(item => item.Value)
            .ToList();
        return Task.FromResult<IReadOnlyList<Document>>(documents);
    }

    public Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        if (document is null) throw new ArgumentNullException(nameof(document));
        _documents[(document.CompanyPublicId, document.Id)] = document;
        return Task.CompletedTask;
    }

    public long NextVersionId() => _nextVersionId++;

    public long NextLinkId() => _nextLinkId++;
}
