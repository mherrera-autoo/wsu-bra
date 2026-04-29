using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class CompanyUnitOfMeasureRepository : ICompanyUnitOfMeasureRepository
{
    private readonly ErpDbContext _dbContext;

    public CompanyUnitOfMeasureRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(CompanyUnitOfMeasure companyUnitOfMeasure, CancellationToken cancellationToken = default)
    {
        await _dbContext.CompanyUnitsOfMeasure.AddAsync(companyUnitOfMeasure, cancellationToken);
    }

    public Task<CompanyUnitOfMeasure?> GetAsync(long companyId, long unitOfMeasureId, CancellationToken cancellationToken = default)
        => _dbContext.CompanyUnitsOfMeasure
            .Include(item => item.UnitOfMeasure)
            .ThenInclude(unit => unit!.Translations)
            .FirstOrDefaultAsync(item => item.CompanyId == companyId && item.UnitOfMeasureId == unitOfMeasureId, cancellationToken);

    public async Task<IReadOnlyList<CompanyUnitOfMeasure>> ListByCompanyAsync(long companyId, bool enabledOnly, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.CompanyUnitsOfMeasure
            .Include(item => item.UnitOfMeasure)
            .ThenInclude(unit => unit!.Translations)
            .Where(item => item.CompanyId == companyId);
        if (enabledOnly)
        {
            query = query.Where(item => item.IsEnabled);
        }

        return await query
            .OrderBy(item => item.SortOrder ?? int.MaxValue)
            .ThenBy(item => item.UnitOfMeasure!.DisplayCode)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> IsEnabledAsync(long companyId, long unitOfMeasureId, CancellationToken cancellationToken = default)
        => _dbContext.CompanyUnitsOfMeasure.AnyAsync(
            item => item.CompanyId == companyId && item.UnitOfMeasureId == unitOfMeasureId && item.IsEnabled,
            cancellationToken);

    public Task<CompanyUnitOfMeasure?> GetDefaultForDimensionAsync(long companyId, UnitOfMeasureDimension dimension, CancellationToken cancellationToken = default)
        => _dbContext.CompanyUnitsOfMeasure
            .Include(item => item.UnitOfMeasure)
            .ThenInclude(unit => unit!.Translations)
            .FirstOrDefaultAsync(item => item.CompanyId == companyId
                && item.Dimension == dimension
                && item.IsDefaultForDimension,
                cancellationToken);
}
