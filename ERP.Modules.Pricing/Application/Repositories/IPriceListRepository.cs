using ERP.Modules.Pricing.Domain;

namespace ERP.Modules.Pricing.Application.Repositories;

public interface IPriceListRepository
{
    Task AddAsync(PriceList priceList, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(long companyId, string name, CancellationToken cancellationToken = default);
    Task<PriceList?> GetDefaultAsync(long companyId, CancellationToken cancellationToken = default);
    Task<PriceList?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PriceList>> ListAsync(long companyId, CancellationToken cancellationToken = default);
    void Remove(PriceList priceList);
}
