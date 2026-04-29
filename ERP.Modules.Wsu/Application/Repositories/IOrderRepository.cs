using ERP.Modules.Wsu.Domain;

namespace ERP.Modules.Wsu.Application.Repositories;

public interface IOrderRepository
{
    Task AddAsync(Order item, CancellationToken cancellationToken = default);
    Task<Order?> GetByPublicIdAsync(Guid companyPublicId, Guid publicId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderListItem>> ListAsync(
        Guid companyPublicId,
        Guid? warehousePublicId,
        OrderType? orderType,
        DateTimeOffset? movementDateFrom,
        DateTimeOffset? movementDateTo,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Guid companyPublicId,
        Guid? warehousePublicId,
        OrderType? orderType,
        DateTimeOffset? movementDateFrom,
        DateTimeOffset? movementDateTo,
        CancellationToken cancellationToken = default);
}
