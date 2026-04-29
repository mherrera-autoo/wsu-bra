using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.MasterData.Application.Services;

public static class FeatureGate
{
    public static async Task EnsureEnabledAsync(
        IFeatureService featureService,
        long companyId,
        string featureKey,
        CancellationToken cancellationToken = default)
    {
        if (!await featureService.IsEnabled(companyId, featureKey, cancellationToken))
        {
            throw new FeatureNotEnabledException(featureKey);
        }
    }

    public static Task EnsureEnabledAsync(
        IFeatureService featureService,
        long companyId,
        FeatureCode featureCode,
        CancellationToken cancellationToken = default)
    {
        return EnsureEnabledAsync(featureService, companyId, featureCode.Code, cancellationToken);
    }
}
