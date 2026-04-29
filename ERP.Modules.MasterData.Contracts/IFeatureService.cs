namespace ERP.Modules.MasterData.Contracts;

public interface IFeatureService
{
    Task<bool> IsEnabled(long companyId, string featureKey, CancellationToken cancellationToken = default);
    Task<IReadOnlySet<string>> GetEnabled(long companyId, CancellationToken cancellationToken = default);
}
