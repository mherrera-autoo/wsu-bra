using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class SubdivisionRepository : ISubdivisionRepository
{
    private readonly ErpDbContext _dbContext;

    public SubdivisionRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Subdivision subdivision, CancellationToken cancellationToken = default)
    {
        await _dbContext.Subdivisions.AddAsync(subdivision, cancellationToken);
    }

    public Task<Subdivision?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.Subdivisions.FirstOrDefaultAsync(subdivision => subdivision.Id == id, cancellationToken);

    public Task<Subdivision?> GetByCodeAsync(long countryId, string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return _dbContext.Subdivisions.FirstOrDefaultAsync(
            subdivision => subdivision.CountryId == countryId && subdivision.Code == normalized,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Subdivision>> ListAsync(
        long countryId,
        short? level,
        long? parentSubdivisionId,
        bool activeOnly,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Subdivisions.AsNoTracking().Where(subdivision => subdivision.CountryId == countryId);
        if (level.HasValue)
        {
            query = query.Where(subdivision => subdivision.Level == level);
        }

        if (parentSubdivisionId.HasValue)
        {
            query = query.Where(subdivision => subdivision.ParentSubdivisionId == parentSubdivisionId);
        }

        if (activeOnly)
        {
            query = query.Where(subdivision => subdivision.IsActive);
        }

        return await query.OrderBy(subdivision => subdivision.Name).ToListAsync(cancellationToken);
    }
}
