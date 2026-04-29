using ERP.Modules.MasterData.Application.Services;
using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ERP.Persistence.Services;

public sealed class FeatureCatalogService : IFeatureCatalogService
{
    private readonly ErpDbContext _dbContext;
    private readonly IMemoryCache _cache;

    private static readonly MemoryCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
    };

    public FeatureCatalogService(ErpDbContext dbContext, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task<FeatureCatalog?> GetByPublicId(Guid featurePublicId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"feature-catalog:public-id:{featurePublicId}";
        if (_cache.TryGetValue(cacheKey, out FeatureCatalog? cached) && cached is not null)
        {
            return cached;
        }

        var feature = await _dbContext.FeatureCatalog
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.PublicId == featurePublicId, cancellationToken);

        if (feature is not null)
        {
            _cache.Set(cacheKey, feature, CacheOptions);
            _cache.Set($"feature-catalog:code:{feature.Code}", feature, CacheOptions);
        }

        return feature;
    }

    public async Task<FeatureCatalog?> GetByCode(FeatureCode code, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"feature-catalog:code:{code.Code}";
        if (_cache.TryGetValue(cacheKey, out FeatureCatalog? cached) && cached is not null)
        {
            return cached;
        }

        var feature = await _dbContext.FeatureCatalog
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Code == code.Code, cancellationToken);

        if (feature is not null)
        {
            _cache.Set(cacheKey, feature, CacheOptions);
            _cache.Set($"feature-catalog:public-id:{feature.PublicId}", feature, CacheOptions);
        }

        return feature;
    }

    public async Task<IReadOnlyList<FeatureCatalog>> ListActive(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "feature-catalog:active";
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<FeatureCatalog>? cached) && cached is not null)
        {
            return cached;
        }

        var features = await _dbContext.FeatureCatalog
            .AsNoTracking()
            .Where(item => item.IsActive)
            .OrderBy(item => item.Code)
            .ToListAsync(cancellationToken);

        _cache.Set(cacheKey, features, CacheOptions);
        return features;
    }

    public async Task<IReadOnlyList<FeatureCatalog>> ListAll(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "feature-catalog:all";
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<FeatureCatalog>? cached) && cached is not null)
        {
            return cached;
        }

        var features = await _dbContext.FeatureCatalog
            .AsNoTracking()
            .OrderBy(item => item.Code)
            .ToListAsync(cancellationToken);

        _cache.Set(cacheKey, features, CacheOptions);
        return features;
    }

    public async Task<bool> SetActive(Guid featurePublicId, bool isActive, CancellationToken cancellationToken = default)
    {
        var feature = await _dbContext.FeatureCatalog
            .SingleOrDefaultAsync(item => item.PublicId == featurePublicId, cancellationToken);

        if (feature is null)
        {
            return false;
        }

        if (feature.IsActive != isActive)
        {
            feature.SetActive(isActive, DateTime.UtcNow);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        _cache.Remove("feature-catalog:all");
        _cache.Remove("feature-catalog:active");
        _cache.Remove($"feature-catalog:public-id:{featurePublicId}");
        _cache.Remove($"feature-catalog:code:{feature.Code}");

        return true;
    }
}
