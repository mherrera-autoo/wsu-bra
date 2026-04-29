using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class CityRepository : ICityRepository
{
    private readonly ErpDbContext _dbContext;

    public CityRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(City city, CancellationToken cancellationToken = default)
    {
        await _dbContext.Cities.AddAsync(city, cancellationToken);
    }

    public Task<City?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.Cities.FirstOrDefaultAsync(city => city.Id == id, cancellationToken);

    public Task<City?> GetByOfficialCodeAsync(long countryId, string officialCode, CancellationToken cancellationToken = default)
    {
        var normalized = officialCode.Trim();
        return _dbContext.Cities.FirstOrDefaultAsync(
            city => city.CountryId == countryId && city.OfficialCode == normalized,
            cancellationToken);
    }

    public Task<City?> GetByNameAsync(long countryId, long? subdivisionId, string name, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim();
        return _dbContext.Cities.FirstOrDefaultAsync(
            city => city.CountryId == countryId
                && city.SubdivisionId == subdivisionId
                && city.Name == normalized,
            cancellationToken);
    }

    public async Task<IReadOnlyList<City>> ListAsync(
        long countryId,
        long? subdivisionId,
        string? search,
        bool activeOnly,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Cities.AsNoTracking().Where(city => city.CountryId == countryId);
        if (subdivisionId.HasValue)
        {
            query = query.Where(city => city.SubdivisionId == subdivisionId);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(city => EF.Functions.ILike(city.Name, pattern));
        }

        if (activeOnly)
        {
            query = query.Where(city => city.IsActive);
        }

        return await query.OrderBy(city => city.Name).ToListAsync(cancellationToken);
    }
}
