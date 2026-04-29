using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.Application.Repositories;

public interface ICompanyFeatureRepository
{
    Task<IReadOnlyList<CompanyFeature>> GetByCompanyAsync(long companyId, CancellationToken cancellationToken = default);
    Task<bool> IsEnabledAsync(long companyId, string featureKey, CancellationToken cancellationToken = default);
    Task AddAsync(CompanyFeature feature, CancellationToken cancellationToken = default);
}
