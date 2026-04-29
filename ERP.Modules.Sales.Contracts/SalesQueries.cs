using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Modules.Sales.Contracts;

public interface ISalesDocumentQuery
{
    Task<SalesDocumentSnapshot?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<SalesDocumentSnapshot?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SalesDocumentSnapshot>> ListAsync(long companyId, CancellationToken cancellationToken = default);
}
