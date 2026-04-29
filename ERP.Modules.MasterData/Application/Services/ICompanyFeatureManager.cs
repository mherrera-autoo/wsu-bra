using ERP.Modules.MasterData.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.MasterData.Application.Services;

public interface ICompanyFeatureManager
{
    Task<IReadOnlyList<CompanyFeature>> GetByCompanyAsync(long companyId, CancellationToken cancellationToken = default);
    Task<Result<bool>> EnableAsync(
        long companyId,
        string featureKey,
        long changedByUserId,
        string? reason,
        string? correlationId,
        CancellationToken cancellationToken = default);
    Task<Result<bool>> DisableAsync(
        long companyId,
        string featureKey,
        long changedByUserId,
        string? reason,
        string? correlationId,
        CancellationToken cancellationToken = default);
    Task<Result<bool>> ReplaceSetAsync(
        long companyId,
        IEnumerable<string> features,
        long changedByUserId,
        string? reason,
        string? correlationId,
        CancellationToken cancellationToken = default);
}
