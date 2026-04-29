using ERP.Shared.Domain;

namespace ERP.Modules.MasterData.Domain;

public sealed class UnitOfMeasure : Entity
{
    public string CanonicalCode { get; private set; } = null!;
    public string DisplayCode { get; private set; } = null!;
    public UnitOfMeasureDimension Dimension { get; private set; }
    public bool IsBaseUnit { get; private set; }
    public decimal FactorToBase { get; private set; }
    public int PrecisionScale { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int? SortOrder { get; private set; }
    public IReadOnlyCollection<UnitOfMeasureTranslation> Translations => _translations;
    public IReadOnlyCollection<UnitOfMeasureExternalMapping> ExternalMappings => _externalMappings;

    private readonly List<UnitOfMeasureTranslation> _translations = new();
    private readonly List<UnitOfMeasureExternalMapping> _externalMappings = new();

    private UnitOfMeasure() { }

    public static UnitOfMeasure Create(
        string canonicalCode,
        string displayCode,
        UnitOfMeasureDimension dimension,
        bool isBaseUnit,
        decimal factorToBase,
        int precisionScale,
        bool isActive,
        int? sortOrder)
        => new()
        {
            CanonicalCode = canonicalCode.Trim().ToLowerInvariant(),
            DisplayCode = displayCode.Trim().ToUpperInvariant(),
            Dimension = dimension,
            IsBaseUnit = isBaseUnit,
            FactorToBase = factorToBase,
            PrecisionScale = precisionScale,
            IsActive = isActive,
            SortOrder = sortOrder
        };

    public void Update(
        string displayCode,
        UnitOfMeasureDimension dimension,
        bool isBaseUnit,
        decimal factorToBase,
        int precisionScale,
        bool isActive,
        int? sortOrder)
    {
        DisplayCode = displayCode.Trim().ToUpperInvariant();
        Dimension = dimension;
        IsBaseUnit = isBaseUnit;
        FactorToBase = factorToBase;
        PrecisionScale = precisionScale;
        IsActive = isActive;
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }
}
