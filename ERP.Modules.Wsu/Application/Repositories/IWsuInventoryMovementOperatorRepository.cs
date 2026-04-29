using ERP.Modules.Wsu.Domain;

namespace ERP.Modules.Wsu.Application.Repositories;

public interface IWsuInventoryMovementOperatorRepository
{
    Task<IReadOnlyList<long>> ListMovementIdsByOrderIdAsync(long orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ListOperatorCodesByOrderIdAsync(long orderId, CancellationToken cancellationToken = default);
    Task DeleteByMovementIdsAsync(IReadOnlyCollection<long> movementIds, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IReadOnlyCollection<WsuInventoryMovementOperator> items, CancellationToken cancellationToken = default);
}
