using ERP.Modules.FixedAssets.Application.Repositories;
using ERP.Modules.FixedAssets.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class AssetCategoryRepository : IAssetCategoryRepository
{
    private readonly ErpDbContext _dbContext;

    public AssetCategoryRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(AssetCategory category, CancellationToken cancellationToken = default)
        => await _dbContext.AssetCategories.AddAsync(category, cancellationToken);

    public Task<AssetCategory?> GetAsync(long companyId, long categoryId, CancellationToken cancellationToken = default)
        => _dbContext.AssetCategories
            .FirstOrDefaultAsync(category => category.CompanyId == companyId && category.Id == categoryId, cancellationToken);
}
