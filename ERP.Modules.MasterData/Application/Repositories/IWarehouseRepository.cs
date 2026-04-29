using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.Application.Repositories;

public interface IWarehouseRepository
{
    Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(long companyId, string code, long? excludeId = null, CancellationToken cancellationToken = default);
    Task<Warehouse?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Warehouse>> ListAsync(long companyId, CancellationToken cancellationToken = default);
    void Remove(Warehouse warehouse);
}
