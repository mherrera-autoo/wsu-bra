using ERP.Modules.Pricing.Application.Repositories;
using ERP.Modules.Pricing.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class PriceListRepository : IPriceListRepository
{
    private readonly ErpDbContext _dbContext;

    public PriceListRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(PriceList priceList, CancellationToken cancellationToken = default)
    {
        await _dbContext.PriceLists.AddAsync(priceList, cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(long companyId, string name, CancellationToken cancellationToken = default)
        => _dbContext.PriceLists.AnyAsync(list =>
            list.CompanyId == companyId
            && list.Name == name,
            cancellationToken);

    public Task<PriceList?> GetDefaultAsync(long companyId, CancellationToken cancellationToken = default)
        => _dbContext.PriceLists.FirstOrDefaultAsync(list => list.CompanyId == companyId && list.IsDefault, cancellationToken);

    public Task<PriceList?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => _dbContext.PriceLists.FirstOrDefaultAsync(list => list.CompanyId == companyId && list.Id == id, cancellationToken);

    public async Task<IReadOnlyList<PriceList>> ListAsync(long companyId, CancellationToken cancellationToken = default)
        => await _dbContext.PriceLists.AsNoTracking()
            .Where(list => list.CompanyId == companyId)
            .OrderBy(list => list.Name)
            .ToListAsync(cancellationToken);

    public void Remove(PriceList priceList)
    {
        _dbContext.PriceLists.Remove(priceList);
    }
}
