using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class CompanyFeatureRepository : ICompanyFeatureRepository
{
    private readonly ErpDbContext _dbContext;

    public CompanyFeatureRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CompanyFeature>> GetByCompanyAsync(long companyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CompanyFeatures
            .Where(feature => feature.CompanyId == companyId)
            .OrderBy(feature => feature.FeaturePublicId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsEnabledAsync(long companyId, string featureKey, CancellationToken cancellationToken = default)
    {
        if (!FeatureCode.TryFromCode(featureKey, out var code))
        {
            return false;
        }

        return await _dbContext.CompanyFeatures
            .AnyAsync(
                feature =>
                    feature.CompanyId == companyId
                    && feature.FeaturePublicId == code.PublicId
                    && feature.IsActive,
                cancellationToken);
    }

    public async Task AddAsync(CompanyFeature feature, CancellationToken cancellationToken = default)
    {
        await _dbContext.CompanyFeatures.AddAsync(feature, cancellationToken);
    }
}
