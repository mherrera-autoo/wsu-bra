using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class CountryRepository : ICountryRepository
{
    private readonly ErpDbContext _dbContext;

    public CountryRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Country country, CancellationToken cancellationToken = default)
    {
        await _dbContext.Countries.AddAsync(country, cancellationToken);
    }

    public Task<Country?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.Countries.FirstOrDefaultAsync(country => country.Id == id, cancellationToken);

    public Task<Country?> GetByIso2Async(string iso2, CancellationToken cancellationToken = default)
    {
        var normalized = iso2.Trim().ToUpperInvariant();
        return _dbContext.Countries.FirstOrDefaultAsync(country => country.Iso2 == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<Country>> ListAsync(bool activeOnly, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Countries.AsNoTracking();
        if (activeOnly)
        {
            query = query.Where(country => country.IsActive);
        }

        return await query.OrderBy(country => country.Name).ToListAsync(cancellationToken);
    }
}
