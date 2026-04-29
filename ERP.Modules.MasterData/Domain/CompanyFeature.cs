using ERP.Shared.Domain;

namespace ERP.Modules.MasterData.Domain;

public sealed class CompanyFeature : CompanyEntity
{
    public Guid FeaturePublicId { get; private set; }
    public bool IsActive { get; private set; }

    private CompanyFeature() { }

    public static CompanyFeature Create(long companyId, Guid featurePublicId, bool isActive, DateTime now)
    {
        if (featurePublicId == Guid.Empty)
        {
            throw new ArgumentException("FeaturePublicId is required.", nameof(featurePublicId));
        }

        return new CompanyFeature
        {
            CompanyId = companyId,
            FeaturePublicId = featurePublicId,
            IsActive = isActive,
            UpdatedAt = now
        };
    }

    public void SetActive(bool isActive, DateTime now)
    {
        IsActive = isActive;
        UpdatedAt = now;
    }
}
