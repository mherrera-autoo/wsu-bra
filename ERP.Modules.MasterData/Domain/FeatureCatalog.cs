using ERP.Shared.Domain;

namespace ERP.Modules.MasterData.Domain;

public sealed class FeatureCatalog : Entity
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private FeatureCatalog() { }

    public static FeatureCatalog Create(
        FeatureCode code,
        string name,
        string? description,
        bool isActive,
        DateTime now)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        return new FeatureCatalog
        {
            PublicId = code.PublicId,
            Code = code.Code,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            IsActive = isActive,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void SetActive(bool isActive, DateTime now)
    {
        IsActive = isActive;
        UpdatedAt = now;
    }

    public void UpdateCatalog(FeatureCode code, string name, string? description, bool isActive, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        PublicId = code.PublicId;
        Code = code.Code;
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        IsActive = isActive;
        UpdatedAt = now;
    }
}
