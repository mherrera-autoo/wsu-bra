using ERP.Modules.Wsu.Domain;

namespace ERP.Modules.Wsu.Application.Repositories;

public interface IWsuOrderMovementOperatorRepository
{
    Task<IReadOnlyList<string>> ListOperatorCodesByOrderIdAsync(long orderId, CancellationToken cancellationToken = default);
    Task DeleteByOrderIdAsync(long orderId, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IReadOnlyCollection<WsuOrderMovementOperator> items, CancellationToken cancellationToken = default);
}
