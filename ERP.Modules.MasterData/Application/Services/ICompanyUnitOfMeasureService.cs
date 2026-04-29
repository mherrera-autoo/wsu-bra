using ERP.Modules.MasterData.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.MasterData.Application.Services;

public interface ICompanyUnitOfMeasureService
{
    Task<IReadOnlyList<CompanyUnitOfMeasure>> ListEnabledAsync(long companyId, CancellationToken cancellationToken = default);
    Task<Result<CompanyUnitOfMeasure>> EnableAsync(
        long companyId,
        long unitOfMeasureId,
        string? displayNameOverride,
        int? sortOrder,
        CancellationToken cancellationToken = default);
    Task<Result> DisableAsync(long companyId, long unitOfMeasureId, CancellationToken cancellationToken = default);
    Task<Result> ReplaceEnabledAsync(long companyId, IReadOnlyCollection<long> unitOfMeasureIds, CancellationToken cancellationToken = default);
    Task<Result> SetDefaultAsync(long companyId, UnitOfMeasureDimension dimension, long unitOfMeasureId, CancellationToken cancellationToken = default);
}
