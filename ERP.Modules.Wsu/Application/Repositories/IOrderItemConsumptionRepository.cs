using ERP.Modules.Wsu.Domain;

namespace ERP.Modules.Wsu.Application.Repositories;

public interface IOrderItemConsumptionRepository
{
    Task AddAsync(OrderItemConsumption item, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderItemConsumption>> ListByOutOrderItemIdAsync(long outOrderItemId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalCostByOutOrderItemIdAsync(long outOrderItemId, CancellationToken cancellationToken = default);
}
