using ERP.Modules.Inventory.Domain;

namespace ERP.Modules.Inventory.Application.Repositories;

public interface IStockRepository
{
    Task<Stock?> GetAsync(long companyId, long productId, long warehouseId, CancellationToken cancellationToken = default);
    Task UpsertAsync(Stock stock, CancellationToken cancellationToken = default);
}
