using ERP.Modules.Wms.Domain;

namespace ERP.Modules.Wms.Application.Repositories;

public interface IWarehouseOperationRepository
{
    Task AddAsync(WarehouseOperation operation, CancellationToken cancellationToken = default);
    Task UpdateAsync(WarehouseOperation operation, CancellationToken cancellationToken = default);
    Task<WarehouseOperation?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WarehouseOperation>> ListByWarehouseAsync(long companyId, long warehouseId, CancellationToken cancellationToken = default);
}
