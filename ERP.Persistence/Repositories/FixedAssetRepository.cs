using ERP.Modules.FixedAssets.Application.Repositories;
using ERP.Modules.FixedAssets.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class FixedAssetRepository : IFixedAssetRepository
{
    private readonly ErpDbContext _dbContext;

    public FixedAssetRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(FixedAsset asset, CancellationToken cancellationToken = default)
        => await _dbContext.FixedAssets.AddAsync(asset, cancellationToken);

    public Task<FixedAsset?> GetAsync(long companyId, long assetId, CancellationToken cancellationToken = default)
        => _dbContext.FixedAssets
            .FirstOrDefaultAsync(asset => asset.CompanyId == companyId && asset.Id == assetId, cancellationToken);

    public Task UpdateAsync(FixedAsset asset, CancellationToken cancellationToken = default)
    {
        _dbContext.FixedAssets.Update(asset);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<FixedAsset>> ListActiveAsync(long companyId, CancellationToken cancellationToken = default)
        => await _dbContext.FixedAssets
            .AsNoTracking()
            .Where(asset => asset.CompanyId == companyId && asset.Status == FixedAssetStatus.Active)
            .OrderBy(asset => asset.Name)
            .ToListAsync(cancellationToken);
}
