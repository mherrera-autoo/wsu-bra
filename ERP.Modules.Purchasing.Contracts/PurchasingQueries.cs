using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Modules.Purchasing.Contracts;

public interface IPurchaseOrderQuery
{
    Task<PurchaseOrderSnapshot?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<PurchaseOrderSnapshot?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PurchaseOrderSnapshot>> ListAsync(long companyId, CancellationToken cancellationToken = default);
}

public interface IGoodsReceiptQuery
{
    Task<GoodsReceiptSnapshot?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GoodsReceiptSnapshot>> ListAsync(long companyId, CancellationToken cancellationToken = default);
}
