using ERP.Modules.Sales.Domain;

namespace ERP.Modules.Sales.Application.Repositories;

public interface ISalesDocumentRepository
{
    Task AddAsync(SalesDocument document, CancellationToken cancellationToken = default);
    Task<SalesDocument?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SalesDocument>> ListAsync(long companyId, CancellationToken cancellationToken = default);
}
