using ERP.Modules.Pricing.Domain;

namespace ERP.Modules.Pricing.Application.Repositories;

public interface IPriceListItemRepository
{
    Task AddAsync(PriceListItem priceListItem, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(long companyId, long priceListId, long productId, CancellationToken cancellationToken = default);
    Task<PriceListItem?> GetByProductAsync(long companyId, long priceListId, long productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PriceListItem>> ListByPriceListAsync(long companyId, long priceListId, CancellationToken cancellationToken = default);
    void Remove(PriceListItem priceListItem);
}
