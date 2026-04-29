using ERP.Modules.Wsu.Domain;

namespace ERP.Modules.Wsu.Application.Repositories;

public interface IOrderItemReconciliationRepository
{
    Task AddAsync(OrderItemReconciliation item, CancellationToken cancellationToken = default);
}
