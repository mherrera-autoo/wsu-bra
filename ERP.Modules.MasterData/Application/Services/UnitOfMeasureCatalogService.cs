using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.MasterData.Application.Services;

public sealed class UnitOfMeasureCatalogService : IUnitOfMeasureCatalog
{
    private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UnitOfMeasureCatalogService(IUnitOfMeasureRepository unitOfMeasureRepository, IUnitOfWork unitOfWork)
    {
        _unitOfMeasureRepository = unitOfMeasureRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<IReadOnlyList<UnitOfMeasure>> ListAsync(CancellationToken cancellationToken = default)
        => _unitOfMeasureRepository.ListAsync(cancellationToken);

    public Task<UnitOfMeasure?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _unitOfMeasureRepository.GetByIdAsync(id, cancellationToken);

    public async Task<Result<UnitOfMeasure>> CreateAsync(
        string canonicalCode,
        string displayCode,
        UnitOfMeasureDimension dimension,
        bool isBaseUnit,
        decimal factorToBase,
        int precisionScale,
        bool isActive,
        int? sortOrder,
        CancellationToken cancellationToken = default)
    {
        if (await _unitOfMeasureRepository.ExistsByCanonicalCodeAsync(canonicalCode, cancellationToken: cancellationToken))
        {
            return Result<UnitOfMeasure>.Fail($"Unit of measure code '{canonicalCode}' already exists.");
        }

        if (isBaseUnit && factorToBase != 1m)
        {
            return Result<UnitOfMeasure>.Fail("Base units must have a factor of 1.");
        }

        var unitOfMeasure = UnitOfMeasure.Create(
            canonicalCode,
            displayCode,
            dimension,
            isBaseUnit,
            factorToBase,
            precisionScale,
            isActive,
            sortOrder);
        await _unitOfMeasureRepository.AddAsync(unitOfMeasure, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<UnitOfMeasure>.Ok(unitOfMeasure);
    }

    public async Task<Result<UnitOfMeasure>> UpdateAsync(
        long id,
        string displayCode,
        UnitOfMeasureDimension dimension,
        bool isBaseUnit,
        decimal factorToBase,
        int precisionScale,
        bool isActive,
        int? sortOrder,
        CancellationToken cancellationToken = default)
    {
        var unitOfMeasure = await _unitOfMeasureRepository.GetByIdAsync(id, cancellationToken);
        if (unitOfMeasure is null)
        {
            return Result<UnitOfMeasure>.Fail("Unit of measure not found.");
        }

        if (isBaseUnit && factorToBase != 1m)
        {
            return Result<UnitOfMeasure>.Fail("Base units must have a factor of 1.");
        }

        unitOfMeasure.Update(
            displayCode,
            dimension,
            isBaseUnit,
            factorToBase,
            precisionScale,
            isActive,
            sortOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<UnitOfMeasure>.Ok(unitOfMeasure);
    }
}
