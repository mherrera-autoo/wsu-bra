using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.MasterData.Application.Services;

public sealed class CompanyUnitOfMeasureService : ICompanyUnitOfMeasureService
{
    private readonly ICompanyUnitOfMeasureRepository _companyUnitRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompanyUnitOfMeasureService(
        ICompanyUnitOfMeasureRepository companyUnitRepository,
        IProductRepository productRepository,
        IUnitOfMeasureRepository unitOfMeasureRepository,
        IUnitOfWork unitOfWork)
    {
        _companyUnitRepository = companyUnitRepository;
        _productRepository = productRepository;
        _unitOfMeasureRepository = unitOfMeasureRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<IReadOnlyList<CompanyUnitOfMeasure>> ListEnabledAsync(long companyId, CancellationToken cancellationToken = default)
        => _companyUnitRepository.ListByCompanyAsync(companyId, true, cancellationToken);

    public async Task<Result<CompanyUnitOfMeasure>> EnableAsync(
        long companyId,
        long unitOfMeasureId,
        string? displayNameOverride,
        int? sortOrder,
        CancellationToken cancellationToken = default)
    {
        var unitOfMeasure = await _unitOfMeasureRepository.GetByIdAsync(unitOfMeasureId, cancellationToken);
        if (unitOfMeasure is null)
        {
            return Result<CompanyUnitOfMeasure>.Fail("Unit of measure not found.");
        }

        var existing = await _companyUnitRepository.GetAsync(companyId, unitOfMeasureId, cancellationToken);
        if (existing is null)
        {
            var companyUnit = CompanyUnitOfMeasure.Create(
                companyId,
                unitOfMeasureId,
                unitOfMeasure.Dimension,
                displayNameOverride,
                sortOrder,
                true,
                false);
            await _companyUnitRepository.AddAsync(companyUnit, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<CompanyUnitOfMeasure>.Ok(companyUnit);
        }

        if (!existing.IsEnabled)
        {
            existing.Enable(displayNameOverride, sortOrder);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<CompanyUnitOfMeasure>.Ok(existing);
    }

    public async Task<Result> DisableAsync(long companyId, long unitOfMeasureId, CancellationToken cancellationToken = default)
    {
        var existing = await _companyUnitRepository.GetAsync(companyId, unitOfMeasureId, cancellationToken);
        if (existing is null || !existing.IsEnabled)
        {
            return Result.Fail("Unit of measure not enabled.");
        }

        if (await _productRepository.ExistsByUnitOfMeasureAsync(companyId, unitOfMeasureId, cancellationToken))
        {
            return Result.Fail("Unit of measure is in use and cannot be disabled.");
        }

        existing.Disable();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> ReplaceEnabledAsync(long companyId, IReadOnlyCollection<long> unitOfMeasureIds, CancellationToken cancellationToken = default)
    {
        var ids = new HashSet<long>(unitOfMeasureIds);
        var existingUnits = await _unitOfMeasureRepository.ListAsync(cancellationToken);
        var missing = ids.Except(existingUnits.Select(unit => unit.Id)).ToList();
        if (missing.Count != 0)
        {
            return Result.Fail($"Unit of measure not found: {string.Join(", ", missing)}.");
        }

        var existing = await _companyUnitRepository.ListByCompanyAsync(companyId, false, cancellationToken);
        var existingById = existing.ToDictionary(item => item.UnitOfMeasureId);
        var toDisable = existing.Where(item => item.IsEnabled && !ids.Contains(item.UnitOfMeasureId)).ToList();
        foreach (var item in toDisable)
        {
            if (await _productRepository.ExistsByUnitOfMeasureAsync(companyId, item.UnitOfMeasureId, cancellationToken))
            {
                return Result.Fail($"Unit of measure '{item.UnitOfMeasureId}' is in use and cannot be disabled.");
            }
        }

        foreach (var item in toDisable)
        {
            item.Disable();
        }

        foreach (var unitId in ids)
        {
            if (existingById.TryGetValue(unitId, out var existingUnit))
            {
                if (!existingUnit.IsEnabled)
                {
                    existingUnit.Enable(existingUnit.DisplayNameOverride, existingUnit.SortOrder);
                }

                continue;
            }

            var unit = existingUnits.First(uom => uom.Id == unitId);
            await _companyUnitRepository.AddAsync(
                CompanyUnitOfMeasure.Create(companyId, unitId, unit.Dimension, null, null, true, false),
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> SetDefaultAsync(long companyId, UnitOfMeasureDimension dimension, long unitOfMeasureId, CancellationToken cancellationToken = default)
    {
        var unit = await _unitOfMeasureRepository.GetByIdAsync(unitOfMeasureId, cancellationToken);
        if (unit is null)
        {
            return Result.Fail("Unit of measure not found.");
        }

        if (unit.Dimension != dimension)
        {
            return Result.Fail("Unit of measure dimension does not match.");
        }

        var companyUnit = await _companyUnitRepository.GetAsync(companyId, unitOfMeasureId, cancellationToken);
        if (companyUnit is null || !companyUnit.IsEnabled)
        {
            return Result.Fail("Unit of measure is not enabled for the company.");
        }

        var existingDefault = await _companyUnitRepository.GetDefaultForDimensionAsync(companyId, dimension, cancellationToken);
        if (existingDefault is not null && existingDefault.UnitOfMeasureId != unitOfMeasureId)
        {
            return Result.Fail("A default unit already exists for this dimension.");
        }

        if (!companyUnit.IsDefaultForDimension)
        {
            companyUnit.SetDefaultForDimension(true);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Ok();
    }
}
