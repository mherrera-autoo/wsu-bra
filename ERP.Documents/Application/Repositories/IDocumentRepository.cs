using ERP.Documents.Domain;

namespace ERP.Documents.Application.Repositories;

public interface IDocumentRepository
{
    Task AddAsync(Document document, CancellationToken cancellationToken = default);
    Task<Document?> GetAsync(Guid companyPublicId, long documentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Document>> ListAsync(Guid companyPublicId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Document document, CancellationToken cancellationToken = default);
    long NextVersionId();
    long NextLinkId();
}
