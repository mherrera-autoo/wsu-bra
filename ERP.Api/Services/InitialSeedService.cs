using ERP.Modules.MasterData.Application.Services;
using ERP.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ERP.Shared.Application;

namespace ERP.Api.Services;

public sealed class InitialSeedService
{
    private const string DefaultChartVersion = "CL_BASIC";
    private readonly IUnitOfMeasureSeedService _unitOfMeasureSeedService;
    private readonly IGeoSeedService _geoSeedService;
    private readonly ERP.Modules.Accounting.Application.Services.AccountingChartSeedService _chartSeedService;
    private readonly ICurrencySeedService _currencySeedService;
    private readonly ErpDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly FeatureCatalogSeedService _featureCatalogSeedService;
    private readonly RbacSeedService _rbacSeedService;
    private readonly ILogger<InitialSeedService> _logger;

    public InitialSeedService(
        IUnitOfMeasureSeedService unitOfMeasureSeedService,
        IGeoSeedService geoSeedService,
        ERP.Modules.Accounting.Application.Services.AccountingChartSeedService chartSeedService,
        ICurrencySeedService currencySeedService,
        ErpDbContext dbContext,
        IConfiguration configuration,
        FeatureCatalogSeedService featureCatalogSeedService,
        RbacSeedService rbacSeedService,
        ILogger<InitialSeedService> logger)
    {
        _unitOfMeasureSeedService = unitOfMeasureSeedService;
        _geoSeedService = geoSeedService;
        _chartSeedService = chartSeedService;
        _currencySeedService = currencySeedService;
        _dbContext = dbContext;
        _configuration = configuration;
        _featureCatalogSeedService = featureCatalogSeedService;
        _rbacSeedService = rbacSeedService;
        _logger = logger;
    }

    public async Task<Result> SeedGlobalCatalogAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting global catalog seeding");

        // 1) Migraciones
        await _dbContext.Database.MigrateAsync(cancellationToken);

        // 2) Unidades de medida
        var unitSeed = await _unitOfMeasureSeedService.SeedGlobalStandardUnitsAsync(cancellationToken);
        if (!unitSeed.Success)
            throw new InvalidOperationException(unitSeed.Error ?? "Failed to seed global units of measure.");

        cancellationToken.ThrowIfCancellationRequested();

        // 3) Geografía base
        var geoSeed = await _geoSeedService.SeedAsync(cancellationToken);
        if (!geoSeed.Success)
            throw new InvalidOperationException(geoSeed.Error ?? "Failed to seed geo catalog data.");

        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation("Global catalog seed completed successfully.");

        return Result.Ok();
    }

    public async Task<Result> SeedFeaturesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting feature catalog seeding.");

        await _featureCatalogSeedService.SeedAsync(cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation("Feature catalog seed completed successfully.");
        return Result.Ok();
    }

    public async Task<Result> SeedRbacAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting RBAC seeding.");

        var bootstrapCompanyId = await GetBootstrapCompanyIdIfExistsAsync(cancellationToken);
        await _rbacSeedService.SeedAsync(bootstrapCompanyId, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation("RBAC seed completed successfully.");
        return Result.Ok();
    }

    public async Task<Result> SeedRbacOthersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting supplementary RBAC seeding.");

        var bootstrapCompanyId = await GetBootstrapCompanyIdIfExistsAsync(cancellationToken);
        await _rbacSeedService.SeedOthersAsync(bootstrapCompanyId, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation("Supplementary RBAC seed completed successfully.");
        return Result.Ok();
    }

    public async Task<Result> SeedGlobalAccountingAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting global accounting seed.");

        var chartSeed = await _chartSeedService.EnsureGlobalTemplateAsync(DefaultChartVersion, cancellationToken);
        if (!chartSeed.Success)
            throw new InvalidOperationException(chartSeed.Error ?? "Failed to seed chart of accounts template.");

        cancellationToken.ThrowIfCancellationRequested();

        var currencySeed = await _currencySeedService.SeedAsync(cancellationToken);
        if (!currencySeed.Success)
            throw new InvalidOperationException(currencySeed.Error ?? "Failed to seed currencies.");

        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation("Global accounting seed completed successfully.");
        return Result.Ok();
    }

    private bool TryGetBootstrapCompanyId(out long companyId)
    {
        var companyIdValue = _configuration["Bootstrap:CompanyId"];
        if (!long.TryParse(companyIdValue, out companyId) || companyId <= 0)
        {
            companyId = default;
            return false;
        }

        return true;
    }

    private async Task<long?> GetBootstrapCompanyIdFromCompaniesAsync(CancellationToken cancellationToken)
    {
        var companiesSection = _configuration.GetSection("Bootstrap:Company");
        var companies = companiesSection.Get<List<BootstrapCompanyDefinition>>();
        var firstCompany = companies?.FirstOrDefault();
        if (firstCompany is null)
        {
            return null;
        }

        var taxId = firstCompany.TaxId?.Trim();
        if (string.IsNullOrWhiteSpace(taxId))
        {
            return null;
        }

        var bootstrapCompany = await _dbContext.Companies
            .AsNoTracking()
            .Join(
                _dbContext.TaxEntities.AsNoTracking(),
                company => company.TaxEntityId,
                taxEntity => taxEntity.Id,
                (company, taxEntity) => new { company, taxEntity })
            .FirstOrDefaultAsync(pair => pair.taxEntity.TaxId == taxId, cancellationToken);

        return bootstrapCompany?.company.Id;
    }

    private async Task<long?> GetBootstrapCompanyIdIfExistsAsync(CancellationToken cancellationToken)
    {
        if (!TryGetBootstrapCompanyId(out var companyId))
        {
            companyId = (await GetBootstrapCompanyIdFromCompaniesAsync(cancellationToken)) ?? default;
        }

        if (companyId <= 0)
        {
            _logger.LogWarning("Bootstrap:CompanyId is missing or invalid. Skipping company role seeding.");
            return null;
        }

        var exists = await _dbContext.Companies
            .AsNoTracking()
            .AnyAsync(company => company.Id == companyId, cancellationToken);

        if (!exists)
        {
            _logger.LogWarning("Bootstrap company {CompanyId} was not found. Skipping company role seeding.", companyId);
            return null;
        }

        return companyId;
    }
}
