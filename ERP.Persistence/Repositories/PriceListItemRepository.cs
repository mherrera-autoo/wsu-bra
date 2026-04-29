using ERP.Modules.Pricing.Application.Repositories;
using ERP.Modules.Pricing.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class PriceListItemRepository : IPriceListItemRepository
{
    private readonly ErpDbContext _dbContext;

    public PriceListItemRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(PriceListItem priceListItem, CancellationToken cancellationToken = default)
    {
        await _dbContext.PriceListItems.AddAsync(priceListItem, cancellationToken);
    }

    public Task<bool> ExistsAsync(long companyId, long priceListId, long productId, CancellationToken cancellationToken = default)
        => _dbContext.PriceListItems.AnyAsync(item =>
            item.CompanyId == companyId
            && item.PriceListId == priceListId
            && item.ProductId == productId,
            cancellationToken);

    public Task<PriceListItem?> GetByProductAsync(long companyId, long priceListId, long productId, CancellationToken cancellationToken = default)
        => _dbContext.PriceListItems.FirstOrDefaultAsync(item =>
            item.CompanyId == companyId
            && item.PriceListId == priceListId
            && item.ProductId == productId,
            cancellationToken);

    public async Task<IReadOnlyList<PriceListItem>> ListByPriceListAsync(long companyId, long priceListId, CancellationToken cancellationToken = default)
        => await _dbContext.PriceListItems.AsNoTracking()
            .Where(item => item.CompanyId == companyId && item.PriceListId == priceListId)
            .OrderBy(item => item.ProductId)
            .ToListAsync(cancellationToken);

    public void Remove(PriceListItem priceListItem)
    {
        _dbContext.PriceListItems.Remove(priceListItem);
    }
}
