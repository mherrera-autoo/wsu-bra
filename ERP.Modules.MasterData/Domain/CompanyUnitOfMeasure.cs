namespace ERP.Modules.MasterData.Domain;

public sealed class CompanyUnitOfMeasure
{
    public long CompanyId { get; private set; }
    public long UnitOfMeasureId { get; private set; }
    public UnitOfMeasure? UnitOfMeasure { get; private set; }
    public UnitOfMeasureDimension Dimension { get; private set; }
    public bool IsEnabled { get; private set; } = true;
    public bool IsDefaultForDimension { get; private set; }
    public string? DisplayNameOverride { get; private set; }
    public int? SortOrder { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    private CompanyUnitOfMeasure() { }

    public static CompanyUnitOfMeasure Create(
        long companyId,
        long unitOfMeasureId,
        UnitOfMeasureDimension dimension,
        string? displayNameOverride,
        int? sortOrder,
        bool isEnabled,
        bool isDefaultForDimension)
        => new()
        {
            CompanyId = companyId,
            UnitOfMeasureId = unitOfMeasureId,
            Dimension = dimension,
            DisplayNameOverride = string.IsNullOrWhiteSpace(displayNameOverride) ? null : displayNameOverride.Trim(),
            SortOrder = sortOrder,
            IsEnabled = isEnabled,
            IsDefaultForDimension = isDefaultForDimension
        };

    public void Enable(string? displayNameOverride, int? sortOrder)
    {
        IsEnabled = true;
        DisplayNameOverride = string.IsNullOrWhiteSpace(displayNameOverride) ? null : displayNameOverride.Trim();
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable()
    {
        IsEnabled = false;
        IsDefaultForDimension = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDefaultForDimension(bool isDefault)
    {
        IsDefaultForDimension = isDefault;
        UpdatedAt = DateTime.UtcNow;
    }
}
