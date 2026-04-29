using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Shared.Application;

public interface IDocumentSearch
{
    Task IndexAsync(DocumentIndexRequest request, CancellationToken cancellationToken = default);
    Task RemoveAsync(long companyId, string documentType, string documentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentSearchResult>> SearchAsync(DocumentSearchQuery query, CancellationToken cancellationToken = default);
}
