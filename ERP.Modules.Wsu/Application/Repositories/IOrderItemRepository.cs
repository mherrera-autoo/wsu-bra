using ERP.Modules.Wsu.Domain;

namespace ERP.Modules.Wsu.Application.Repositories;

public interface IOrderItemRepository
{
    Task AddAsync(OrderItem item, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderItem>> ListAvailableInboundForFifoAsync(
        Guid companyPublicId,
        Guid? warehousePublicId,
        Guid productPublicId,
        CancellationToken cancellationToken = default);

    Task<decimal> GetAvailableStockAsync(
        Guid companyPublicId,
        Guid? warehousePublicId,
        Guid productPublicId,
        CancellationToken cancellationToken = default);

    Task<decimal> GetInventoryValuationAsync(
        Guid companyPublicId,
        Guid? warehousePublicId,
        CancellationToken cancellationToken = default);
}
