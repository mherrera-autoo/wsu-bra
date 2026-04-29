using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class UnitOfMeasureRepository : IUnitOfMeasureRepository
{
    private readonly ErpDbContext _dbContext;

    public UnitOfMeasureRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(UnitOfMeasure unitOfMeasure, CancellationToken cancellationToken = default)
    {
        await _dbContext.UnitOfMeasures.AddAsync(unitOfMeasure, cancellationToken);
    }

    public async Task<UnitOfMeasure?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UnitOfMeasures
            .Include(uom => uom.Translations)
            .Include(uom => uom.ExternalMappings)
            .FirstOrDefaultAsync(uom => uom.Id == id, cancellationToken);
    }

    public async Task<UnitOfMeasure?> GetByCanonicalCodeAsync(string canonicalCode, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UnitOfMeasures
            .Include(uom => uom.Translations)
            .Include(uom => uom.ExternalMappings)
            .FirstOrDefaultAsync(
                uom => uom.CanonicalCode == canonicalCode.Trim().ToLowerInvariant(),
                cancellationToken);
    }

    public Task<bool> ExistsByCanonicalCodeAsync(string canonicalCode, long? excludeId = null, CancellationToken cancellationToken = default)
        => _dbContext.UnitOfMeasures.AnyAsync(uom =>
            uom.CanonicalCode == canonicalCode.Trim().ToLowerInvariant()
            && (!excludeId.HasValue || uom.Id != excludeId.Value),
            cancellationToken);

    public async Task<IReadOnlyList<UnitOfMeasure>> ListAsync(CancellationToken cancellationToken = default)
        => await _dbContext.UnitOfMeasures
            .Include(uom => uom.Translations)
            .Include(uom => uom.ExternalMappings)
            .AsNoTracking()
            .OrderBy(uom => uom.SortOrder ?? int.MaxValue)
            .ThenBy(uom => uom.DisplayCode)
            .ToListAsync(cancellationToken);
}
