using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.Application.Services;

public interface ICompanyFeatureService
{
    Task EnableFeature(Guid featurePublicId, long userId, string reason, Guid correlationId, CancellationToken cancellationToken = default);
    Task DisableFeature(Guid featurePublicId, long userId, string reason, Guid correlationId, CancellationToken cancellationToken = default);
    Task UpsertCompanyFeature(Guid companyPublicId, Guid featurePublicId, bool isActive, long userId, string reason, Guid correlationId, CancellationToken cancellationToken = default);
    Task<bool> HasFeature(Guid featurePublicId, CancellationToken cancellationToken = default);
    Task<bool> HasFeature(FeatureCode code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> ListEnabledFeaturePublicIds(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompanyFeatureState>> ListCompanyFeatures(CancellationToken cancellationToken = default);
}
