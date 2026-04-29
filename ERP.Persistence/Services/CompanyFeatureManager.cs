using ERP.Modules.MasterData.Application.Services;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using ERP.Shared.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ERP.Persistence.Services;

public sealed class CompanyFeatureManager : ICompanyFeatureManager
{
    private readonly ErpDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly IFeatureCatalogService _featureCatalogService;

    public CompanyFeatureManager(
        ErpDbContext dbContext,
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        IFeatureCatalogService featureCatalogService)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _featureCatalogService = featureCatalogService;
    }

    public async Task<IReadOnlyList<CompanyFeature>> GetByCompanyAsync(long companyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CompanyFeatures
            .AsNoTracking()
            .Where(feature => feature.CompanyId == companyId)
            .OrderBy(feature => feature.FeaturePublicId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Result<bool>> EnableAsync(
        long companyId,
        string featureKey,
        long changedByUserId,
        string? reason,
        string? correlationId,
        CancellationToken cancellationToken = default)
    {
        if (!FeatureCode.TryFromCode(featureKey, out var code))
        {
            return Result<bool>.Fail("Feature code is not supported.");
        }

        var changed = false;
        var now = DateTime.UtcNow;
        var featurePublicId = code.PublicId;
        var parsedCorrelationId = ParseCorrelationId(correlationId);
        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var catalogFeature = await _featureCatalogService.GetByPublicId(featurePublicId, token);
            if (catalogFeature is null || !catalogFeature.IsActive)
            {
                return Result.Fail("Feature does not exist in active catalog.");
            }

            var exists = await _dbContext.CompanyFeatures
                .SingleOrDefaultAsync(
                    feature => feature.CompanyId == companyId && feature.FeaturePublicId == featurePublicId,
                    token);

            if (exists is not null)
            {
                if (!exists.IsActive)
                {
                    exists.SetActive(true, now);
                    changed = true;
                }

                await _dbContext.CompanyFeatureAudits.AddAsync(
                    CompanyFeatureAudit.Create(companyId, featurePublicId, CompanyFeatureAuditAction.Enabled, now, changedByUserId, reason, parsedCorrelationId),
                    token);
                return Result.Ok();
            }

            var feature = CompanyFeature.Create(companyId, featurePublicId, true, now);
            await _dbContext.CompanyFeatures.AddAsync(feature, token);
            await _dbContext.CompanyFeatureAudits.AddAsync(
                CompanyFeatureAudit.Create(companyId, featurePublicId, CompanyFeatureAuditAction.Enabled, now, changedByUserId, reason, parsedCorrelationId),
                token);
            changed = true;
            return Result.Ok();
        }, cancellationToken);

        if (!result.Success)
        {
            return Result<bool>.Fail(result.Error ?? "Failed to enable company feature.");
        }

        if (changed)
        {
            _cache.Remove(CompanyFeatureCacheKeys.CompanyFeatures(companyId));
        }

        return Result<bool>.Ok(changed);
    }

    public async Task<Result<bool>> DisableAsync(
        long companyId,
        string featureKey,
        long changedByUserId,
        string? reason,
        string? correlationId,
        CancellationToken cancellationToken = default)
    {
        if (!FeatureCode.TryFromCode(featureKey, out var code))
        {
            return Result<bool>.Fail("Feature code is not supported.");
        }

        var changed = false;
        var now = DateTime.UtcNow;
        var featurePublicId = code.PublicId;
        var parsedCorrelationId = ParseCorrelationId(correlationId);
        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var existing = await _dbContext.CompanyFeatures
                .SingleOrDefaultAsync(
                    feature => feature.CompanyId == companyId && feature.FeaturePublicId == featurePublicId,
                    token);
            if (existing is null)
            {
                await _dbContext.CompanyFeatureAudits.AddAsync(
                    CompanyFeatureAudit.Create(companyId, featurePublicId, CompanyFeatureAuditAction.Disabled, now, changedByUserId, reason, parsedCorrelationId),
                    token);
                return Result.Ok();
            }

            if (existing.IsActive)
            {
                existing.SetActive(false, now);
                changed = true;
            }

            await _dbContext.CompanyFeatureAudits.AddAsync(
                CompanyFeatureAudit.Create(companyId, featurePublicId, CompanyFeatureAuditAction.Disabled, now, changedByUserId, reason, parsedCorrelationId),
                token);
            return Result.Ok();
        }, cancellationToken);

        if (!result.Success)
        {
            return Result<bool>.Fail(result.Error ?? "Failed to disable company feature.");
        }

        if (changed)
        {
            _cache.Remove(CompanyFeatureCacheKeys.CompanyFeatures(companyId));
        }

        return Result<bool>.Ok(changed);
    }

    public async Task<Result<bool>> ReplaceSetAsync(
        long companyId,
        IEnumerable<string> features,
        long changedByUserId,
        string? reason,
        string? correlationId,
        CancellationToken cancellationToken = default)
    {
        var normalized = features
            .Select(feature => FeatureCode.TryFromCode(feature, out var code) ? code.PublicId : Guid.Empty)
            .Where(featurePublicId => featurePublicId != Guid.Empty)
            .Distinct()
            .ToList();

        var changed = false;
        var now = DateTime.UtcNow;
        var parsedCorrelationId = ParseCorrelationId(correlationId);
        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var existing = await _dbContext.CompanyFeatures
                .Where(feature => feature.CompanyId == companyId)
                .ToListAsync(token);

            var byPublicId = existing.ToDictionary(feature => feature.FeaturePublicId);
            foreach (var featurePublicId in normalized)
            {
                if (byPublicId.TryGetValue(featurePublicId, out var row))
                {
                    if (!row.IsActive)
                    {
                        row.SetActive(true, now);
                        changed = true;
                    }

                    continue;
                }

                await _dbContext.CompanyFeatures.AddAsync(CompanyFeature.Create(companyId, featurePublicId, true, now), token);
                changed = true;
            }

            foreach (var row in existing)
            {
                if (normalized.Contains(row.FeaturePublicId) || !row.IsActive)
                {
                    continue;
                }

                row.SetActive(false, now);
                changed = true;
            }

            await _dbContext.CompanyFeatureAudits.AddAsync(
                CompanyFeatureAudit.Create(companyId, FeatureCode.PharmaceuticalDrogueria.PublicId, CompanyFeatureAuditAction.ReplacedSet, now, changedByUserId, reason, parsedCorrelationId),
                token);

            return Result.Ok();
        }, cancellationToken);

        if (!result.Success)
        {
            return Result<bool>.Fail(result.Error ?? "Failed to replace company features.");
        }

        _cache.Remove(CompanyFeatureCacheKeys.CompanyFeatures(companyId));
        return Result<bool>.Ok(changed);
    }

    private static Guid? ParseCorrelationId(string? correlationId)
    {
        if (Guid.TryParse(correlationId, out var parsed))
        {
            return parsed;
        }

        return null;
    }

}
