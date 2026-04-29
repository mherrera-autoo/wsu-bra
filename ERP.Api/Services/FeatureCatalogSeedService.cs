using ERP.Modules.MasterData.Domain;
using ERP.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERP.Api.Services;

public sealed class FeatureCatalogSeedService
{
    private readonly ErpDbContext _dbContext;
    private readonly ILogger<FeatureCatalogSeedService> _logger;

    public FeatureCatalogSeedService(ErpDbContext dbContext, ILogger<FeatureCatalogSeedService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var definitions = FeatureCode.Supported.ToArray();
        var featurePublicIds = definitions.Select(feature => feature.PublicId).ToArray();

        var existing = await _dbContext.FeatureCatalog
            .Where(feature => featurePublicIds.Contains(feature.PublicId))
            .ToListAsync(cancellationToken);

        var byPublicId = existing.ToDictionary(feature => feature.PublicId);
        var now = DateTime.UtcNow;
        var created = 0;
        var updated = 0;

        foreach (var definition in definitions)
        {
            if (!byPublicId.TryGetValue(definition.PublicId, out var row))
            {
                _dbContext.FeatureCatalog.Add(FeatureCatalog.Create(
                    definition,
                    definition.Name,
                    definition.Description,
                    definition.DefaultIsActive,
                    now));
                created++;
                continue;
            }

            var needsUpdate = !string.Equals(row.Code, definition.Code, StringComparison.Ordinal)
                || !string.Equals(row.Name, definition.Name, StringComparison.Ordinal)
                || !string.Equals(row.Description, definition.Description, StringComparison.Ordinal)
                || row.IsActive != definition.DefaultIsActive;

            if (!needsUpdate)
            {
                continue;
            }

            row.UpdateCatalog(definition, definition.Name, definition.Description, definition.DefaultIsActive, now);
            updated++;
        }

        if (created == 0 && updated == 0)
        {
            _logger.LogInformation("Feature catalog seed found no changes.");
            return;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Feature catalog seed completed. Created: {Created}, Updated: {Updated}", created, updated);
    }
}
