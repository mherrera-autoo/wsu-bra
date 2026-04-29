using ERP.Modules.MasterData.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.MasterData.Application.Services;

public interface IUnitOfMeasureCatalog
{
    Task<IReadOnlyList<UnitOfMeasure>> ListAsync(CancellationToken cancellationToken = default);
    Task<UnitOfMeasure?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Result<UnitOfMeasure>> CreateAsync(
        string canonicalCode,
        string displayCode,
        UnitOfMeasureDimension dimension,
        bool isBaseUnit,
        decimal factorToBase,
        int precisionScale,
        bool isActive,
        int? sortOrder,
        CancellationToken cancellationToken = default);
    Task<Result<UnitOfMeasure>> UpdateAsync(
        long id,
        string displayCode,
        UnitOfMeasureDimension dimension,
        bool isBaseUnit,
        decimal factorToBase,
        int precisionScale,
        bool isActive,
        int? sortOrder,
        CancellationToken cancellationToken = default);
}
