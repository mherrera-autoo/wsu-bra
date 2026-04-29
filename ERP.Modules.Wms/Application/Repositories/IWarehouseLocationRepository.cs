using ERP.Modules.Wms.Domain;

namespace ERP.Modules.Wms.Application.Repositories;

public interface IWarehouseLocationRepository
{
    Task AddAsync(WarehouseLocation location, CancellationToken cancellationToken = default);
    Task UpdateAsync(WarehouseLocation location, CancellationToken cancellationToken = default);
    Task<WarehouseLocation?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WarehouseLocation>> ListByWarehouseAsync(long companyId, long warehouseId, CancellationToken cancellationToken = default);
}
