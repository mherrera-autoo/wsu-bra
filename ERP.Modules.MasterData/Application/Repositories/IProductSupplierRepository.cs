using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.Application.Repositories;

public interface IProductSupplierRepository
{
    Task<ProductSupplier?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default);
    Task<ProductSupplier?> GetByProductAndSupplierAsync(long companyId, long productId, long supplierId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductSupplier>> ListByProductAsync(long companyId, long productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductSupplier>> ListBySupplierAsync(long companyId, long supplierId, CancellationToken cancellationToken = default);
    Task AddAsync(ProductSupplier productSupplier, CancellationToken cancellationToken = default);
    void Remove(ProductSupplier productSupplier);
}
