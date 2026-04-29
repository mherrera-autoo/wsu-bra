using ERP.Modules.Purchasing.Domain;

namespace ERP.Modules.Purchasing.Application.Repositories;

public interface IPurchaseOrderRepository
{
    Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default);
    Task<PurchaseOrder?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<PurchaseOrder?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PurchaseOrder>> ListAsync(long companyId, CancellationToken cancellationToken = default);
}
