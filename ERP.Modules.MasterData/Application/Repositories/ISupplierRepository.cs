using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.Application.Repositories;

public interface ISupplierRepository
{
    Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(long companyId, string name, long? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsByTaxIdAsync(long companyId, string taxId, long? excludeId = null, CancellationToken cancellationToken = default);
    Task<Supplier?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Supplier>> ListAsync(
        long companyId,
        string? search = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);
    void Remove(Supplier supplier);
}
