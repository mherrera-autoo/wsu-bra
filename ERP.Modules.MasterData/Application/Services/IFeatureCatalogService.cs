using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.Application.Services;

public interface IFeatureCatalogService
{
    Task<FeatureCatalog?> GetByPublicId(Guid featurePublicId, CancellationToken cancellationToken = default);
    Task<FeatureCatalog?> GetByCode(FeatureCode code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FeatureCatalog>> ListAll(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FeatureCatalog>> ListActive(CancellationToken cancellationToken = default);
    Task<bool> SetActive(Guid featurePublicId, bool isActive, CancellationToken cancellationToken = default);
}
