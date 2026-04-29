using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class LocalityRepository : ILocalityRepository
{
    private readonly ErpDbContext _dbContext;

    public LocalityRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Locality locality, CancellationToken cancellationToken = default)
    {
        await _dbContext.Localities.AddAsync(locality, cancellationToken);
    }

    public Task<Locality?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.Localities.FirstOrDefaultAsync(locality => locality.Id == id, cancellationToken);

    public Task<Locality?> GetByNameAsync(long cityId, string name, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim();
        return _dbContext.Localities.FirstOrDefaultAsync(
            locality => locality.CityId == cityId && locality.Name == normalized,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Locality>> ListAsync(long cityId, bool activeOnly, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Localities.AsNoTracking().Where(locality => locality.CityId == cityId);
        if (activeOnly)
        {
            query = query.Where(locality => locality.IsActive);
        }

        return await query.OrderBy(locality => locality.Name).ToListAsync(cancellationToken);
    }
}
