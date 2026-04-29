using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.MasterData.Application.Services;

public interface IUnitOfMeasureSeedService
{
    Task<Result<IReadOnlyList<UnitOfMeasure>>> SeedGlobalStandardUnitsAsync(CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<UnitOfMeasure>>> EnableStandardUnitsForCompanyAsync(long companyId, CancellationToken cancellationToken);
}

public sealed class UnitOfMeasureSeedService : IUnitOfMeasureSeedService
{
    private readonly IUnitOfMeasureCatalog _catalog;
    private readonly IUnitOfMeasureTranslationRepository _translationRepository;
    private readonly IUnitOfMeasureExternalMappingRepository _externalMappingRepository;
    private readonly ICompanyUnitOfMeasureService _companyService;
    private readonly IUnitOfWork _unitOfWork;

    public UnitOfMeasureSeedService(
        IUnitOfMeasureCatalog catalog,
        IUnitOfMeasureTranslationRepository translationRepository,
        IUnitOfMeasureExternalMappingRepository externalMappingRepository,
        ICompanyUnitOfMeasureService companyService,
        IUnitOfWork unitOfWork)
    {
        _catalog = catalog;
        _translationRepository = translationRepository;
        _externalMappingRepository = externalMappingRepository;
        _companyService = companyService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<UnitOfMeasure>>> SeedGlobalStandardUnitsAsync(
        CancellationToken cancellationToken)
    {
        var seededUnits = await SeedStandardUnitsAsync(cancellationToken);
        if (!seededUnits.Success)
        {
            return seededUnits;
        }

        var unitsByCode = seededUnits.Value!.ToDictionary(unit => unit.CanonicalCode, StringComparer.OrdinalIgnoreCase);

        foreach (var translation in UnitOfMeasureSeedData.Translations)
        {
            if (!unitsByCode.TryGetValue(translation.CanonicalCode, out var unit))
            {
                continue;
            }

            var existing = await _translationRepository.GetAsync(unit.Id, translation.Culture, cancellationToken);
            if (existing is null)
            {
                await _translationRepository.AddAsync(
                    UnitOfMeasureTranslation.Create(unit.Id, translation.Culture, translation.Name, translation.Symbol),
                    cancellationToken);
            }
            else
            {
                existing.Update(translation.Name, translation.Symbol);
            }
        }

        foreach (var mapping in UnitOfMeasureSeedData.ExternalMappings)
        {
            if (!unitsByCode.TryGetValue(mapping.CanonicalCode, out var unit))
            {
                continue;
            }

            var existing = await _externalMappingRepository.GetAsync(mapping.Scheme, mapping.Code, cancellationToken);
            if (existing is null)
            {
                await _externalMappingRepository.AddAsync(
                    UnitOfMeasureExternalMapping.Create(unit.Id, mapping.Scheme, mapping.Code),
                    cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return seededUnits;
    }

    public async Task<Result<IReadOnlyList<UnitOfMeasure>>> EnableStandardUnitsForCompanyAsync(
        long companyId,
        CancellationToken cancellationToken)
    {
        var existing = await _catalog.ListAsync(cancellationToken);
        var existingByCode = existing.ToDictionary(uom => uom.CanonicalCode, StringComparer.OrdinalIgnoreCase);
        var unitsToEnable = new List<UnitOfMeasure>();

        foreach (var code in UnitOfMeasureSeedData.DefaultCompanyUnits)
        {
            if (!existingByCode.TryGetValue(code, out var unit))
            {
                return Result<IReadOnlyList<UnitOfMeasure>>.Fail($"Standard unit '{code}' not found. Run global seed first.");
            }

            unitsToEnable.Add(unit);
        }

        foreach (var unit in unitsToEnable)
        {
            var enableResult = await _companyService.EnableAsync(
                companyId,
                unit.Id,
                null,
                null,
                cancellationToken);
            if (!enableResult.Success)
            {
                return Result<IReadOnlyList<UnitOfMeasure>>.Fail(enableResult.Error);
            }
        }

        foreach (var (dimension, code) in UnitOfMeasureSeedData.DefaultCompanyDimensionUnits)
        {
            if (existingByCode.TryGetValue(code, out var unit))
            {
                var defaultResult = await _companyService.SetDefaultAsync(companyId, dimension, unit.Id, cancellationToken);
                if (!defaultResult.Success)
                {
                    return Result<IReadOnlyList<UnitOfMeasure>>.Fail(defaultResult.Error);
                }
            }
        }

        return Result<IReadOnlyList<UnitOfMeasure>>.Ok(unitsToEnable);
    }

    Task<Result<IReadOnlyList<UnitOfMeasure>>> IUnitOfMeasureSeedService.SeedGlobalStandardUnitsAsync(
        CancellationToken cancellationToken) => SeedGlobalStandardUnitsAsync(cancellationToken);

    Task<Result<IReadOnlyList<UnitOfMeasure>>> IUnitOfMeasureSeedService.EnableStandardUnitsForCompanyAsync(
        long companyId,
        CancellationToken cancellationToken) => EnableStandardUnitsForCompanyAsync(companyId, cancellationToken);

    private async Task<Result<IReadOnlyList<UnitOfMeasure>>> SeedStandardUnitsAsync(
        CancellationToken cancellationToken)
    {
        var seeded = new List<UnitOfMeasure>();
        var existing = await _catalog.ListAsync(cancellationToken);
        var existingByCode = existing.ToDictionary(uom => uom.CanonicalCode, StringComparer.OrdinalIgnoreCase);

        foreach (var seed in UnitOfMeasureSeedData.Units)
        {
            if (existingByCode.TryGetValue(seed.CanonicalCode, out var existingUnit))
            {
                var update = await _catalog.UpdateAsync(
                    existingUnit.Id,
                    seed.DisplayCode,
                    seed.Dimension,
                    seed.IsBaseUnit,
                    seed.FactorToBase,
                    seed.PrecisionScale,
                    seed.IsActive,
                    seed.SortOrder,
                    cancellationToken);
                if (!update.Success)
                {
                    return Result<IReadOnlyList<UnitOfMeasure>>.Fail(update.Error);
                }

                seeded.Add(update.Value!);
                continue;
            }

            var create = await _catalog.CreateAsync(
                seed.CanonicalCode,
                seed.DisplayCode,
                seed.Dimension,
                seed.IsBaseUnit,
                seed.FactorToBase,
                seed.PrecisionScale,
                seed.IsActive,
                seed.SortOrder,
                cancellationToken);

            if (!create.Success)
            {
                return Result<IReadOnlyList<UnitOfMeasure>>.Fail(create.Error);
            }

            seeded.Add(create.Value!);
        }

        return Result<IReadOnlyList<UnitOfMeasure>>.Ok(seeded);
    }
}
