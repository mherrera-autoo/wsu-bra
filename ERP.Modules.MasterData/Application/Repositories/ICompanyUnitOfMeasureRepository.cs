using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.Application.Repositories;

public interface ICompanyUnitOfMeasureRepository
{
    Task AddAsync(CompanyUnitOfMeasure companyUnitOfMeasure, CancellationToken cancellationToken = default);
    Task<CompanyUnitOfMeasure?> GetAsync(long companyId, long unitOfMeasureId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompanyUnitOfMeasure>> ListByCompanyAsync(long companyId, bool enabledOnly, CancellationToken cancellationToken = default);
    Task<bool> IsEnabledAsync(long companyId, long unitOfMeasureId, CancellationToken cancellationToken = default);
    Task<CompanyUnitOfMeasure?> GetDefaultForDimensionAsync(long companyId, UnitOfMeasureDimension dimension, CancellationToken cancellationToken = default);
}
