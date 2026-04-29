using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.Application.Repositories;

public interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySkuAsync(long companyId, string sku, long? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsByBarcodeAsync(long companyId, string barcode, long? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsByPublicIdAsync(long companyId, Guid productPublicId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUnitOfMeasureAsync(long companyId, long unitOfMeasureId, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default);
    Task<Product?> GetByPublicIdAsync(long companyId, Guid productPublicId, CancellationToken cancellationToken = default);
    Task<Product?> GetBySkuAsync(long companyId, string sku, CancellationToken cancellationToken = default);
    Task<Product?> GetByBarcodeAsync(long companyId, string barcode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> ListAsync(long companyId, CancellationToken cancellationToken = default);
    void Remove(Product product);
}
