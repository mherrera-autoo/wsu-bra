using ERP.Modules.MasterData.Application.Services;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using ERP.Shared.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ERP.Persistence.Services;

public sealed class CompanyFeatureService : IFeatureService, ICompanyFeatureService
{
    private readonly ErpDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ICompanyContext _companyContext;
    private readonly IFeatureCatalogService _featureCatalogService;

    private static readonly MemoryCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    };

    public CompanyFeatureService(
        ErpDbContext dbContext,
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ICompanyContext companyContext,
        IFeatureCatalogService featureCatalogService)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _companyContext = companyContext;
        _featureCatalogService = featureCatalogService;
    }

    public async Task EnableFeature(Guid featurePublicId, long userId, string reason, Guid correlationId, CancellationToken cancellationToken = default)
    {
        await SetFeatureState(featurePublicId, isActive: true, userId, reason, correlationId, cancellationToken);
    }

    public async Task DisableFeature(Guid featurePublicId, long userId, string reason, Guid correlationId, CancellationToken cancellationToken = default)
    {
        await SetFeatureState(featurePublicId, isActive: false, userId, reason, correlationId, cancellationToken);
    }

    public async Task<bool> HasFeature(Guid featurePublicId, CancellationToken cancellationToken = default)
    {
        var companyId = GetRequiredCompanyId();
        var activeFeatureIds = await GetActiveFeatureIds(companyId, cancellationToken);
        return activeFeatureIds.Contains(featurePublicId);
    }

    public Task<bool> HasFeature(FeatureCode code, CancellationToken cancellationToken = default)
    {
        return HasFeature(code.PublicId, cancellationToken);
    }

    public async Task<IReadOnlyList<CompanyFeatureState>> ListCompanyFeatures(CancellationToken cancellationToken = default)
    {
        var companyId = GetRequiredCompanyId();
        var now = DateTime.UtcNow;

        var catalog = await _featureCatalogService.ListActive(cancellationToken);
        var states = await _dbContext.CompanyFeatures
            .AsNoTracking()
            .Where(item => item.CompanyId == companyId)
            .ToDictionaryAsync(item => item.FeaturePublicId, item => item, cancellationToken);

        return catalog
            .Select(feature =>
            {
                if (states.TryGetValue(feature.PublicId, out var state))
                {
                    return new CompanyFeatureState(feature.PublicId, state.IsActive, state.UpdatedAt ?? state.CreatedAt);
                }

                return new CompanyFeatureState(feature.PublicId, false, now);
            })
            .OrderBy(item => item.FeaturePublicId)
            .ToArray();
    }

    public async Task<IReadOnlyList<Guid>> ListEnabledFeaturePublicIds(CancellationToken cancellationToken = default)
    {
        var companyId = GetRequiredCompanyId();
        var activeFeatureIds = await GetActiveFeatureIds(companyId, cancellationToken);
        return activeFeatureIds.ToArray();
    }

    public async Task UpsertCompanyFeature(
        Guid companyPublicId,
        Guid featurePublicId,
        bool isActive,
        long userId,
        string reason,
        Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveCompanyId(companyPublicId, cancellationToken);
        await SetFeatureState(companyId, featurePublicId, isActive, userId, reason, correlationId, cancellationToken);
    }

    public async Task<bool> IsEnabled(long companyId, string featureKey, CancellationToken cancellationToken = default)
    {
        if (!FeatureCode.TryFromCode(featureKey, out var featureCode))
        {
            return false;
        }

        var activeFeatureIds = await GetActiveFeatureIds(companyId, cancellationToken);
        return activeFeatureIds.Contains(featureCode.PublicId);
    }

    public async Task<IReadOnlySet<string>> GetEnabled(long companyId, CancellationToken cancellationToken = default)
    {
        var activeFeatureIds = await GetActiveFeatureIds(companyId, cancellationToken);
        var codes = activeFeatureIds
            .Select(id => FeatureCode.TryFromPublicId(id, out var code) ? code.Code : null)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return codes;
    }

    private async Task SetFeatureState(
        Guid featurePublicId,
        bool isActive,
        long userId,
        string reason,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        var companyId = GetRequiredCompanyId();
        await SetFeatureState(companyId, featurePublicId, isActive, userId, reason, correlationId, cancellationToken);
    }

    private async Task SetFeatureState(
        long companyId,
        Guid featurePublicId,
        bool isActive,
        long userId,
        string reason,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Reason is required.", nameof(reason));
        }

        if (correlationId == Guid.Empty)
        {
            throw new ArgumentException("CorrelationId is required.", nameof(correlationId));
        }

        var catalogFeature = await _featureCatalogService.GetByPublicId(featurePublicId, cancellationToken);
        if (catalogFeature is null || !catalogFeature.IsActive)
        {
            throw new InvalidOperationException($"Feature '{featurePublicId}' does not exist in active catalog.");
        }

        var now = DateTime.UtcNow;
        await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var companyFeature = await _dbContext.CompanyFeatures
                .SingleOrDefaultAsync(
                    item => item.CompanyId == companyId && item.FeaturePublicId == featurePublicId,
                    token);

            if (companyFeature is null)
            {
                companyFeature = CompanyFeature.Create(companyId, featurePublicId, isActive, now);
                await _dbContext.CompanyFeatures.AddAsync(companyFeature, token);
            }
            else
            {
                companyFeature.SetActive(isActive, now);
            }

            await _dbContext.CompanyFeatureAudits.AddAsync(
                CompanyFeatureAudit.Create(
                    companyId,
                    featurePublicId,
                    isActive ? CompanyFeatureAuditAction.Enabled : CompanyFeatureAuditAction.Disabled,
                    now,
                    userId,
                    reason,
                    correlationId),
                token);

            return Result.Ok();
        }, cancellationToken);

        _cache.Remove(CompanyFeatureCacheKeys.CompanyFeatures(companyId));
    }

    private long GetRequiredCompanyId()
    {
        if (_companyContext.CompanyId <= 0)
        {
            throw new InvalidOperationException("Company context is required.");
        }

        return _companyContext.CompanyId;
    }

    private async Task<HashSet<Guid>> GetActiveFeatureIds(long companyId, CancellationToken cancellationToken)
    {
        var cacheKey = CompanyFeatureCacheKeys.CompanyFeatures(companyId);
        if (_cache.TryGetValue(cacheKey, out HashSet<Guid>? cached) && cached is not null)
        {
            return cached;
        }

        var activeFeatureIds = await _dbContext.CompanyFeatures
            .AsNoTracking()
            .Where(feature => feature.CompanyId == companyId && feature.IsActive)
            .Select(feature => feature.FeaturePublicId)
            .ToListAsync(cancellationToken);

        var result = activeFeatureIds.ToHashSet();
        _cache.Set(cacheKey, result, CacheOptions);
        return result;
    }

    private async Task<long> ResolveCompanyId(Guid companyPublicId, CancellationToken cancellationToken)
    {
        var companyId = await _dbContext.Companies
            .AsNoTracking()
            .Where(company => company.PublicId == companyPublicId)
            .Select(company => company.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (companyId <= 0)
        {
            throw new InvalidOperationException($"Company '{companyPublicId}' does not exist.");
        }

        return companyId;
    }
}
