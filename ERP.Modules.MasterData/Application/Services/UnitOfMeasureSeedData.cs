using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.Application.Services;

public static class UnitOfMeasureSeedData
{
    public static readonly IReadOnlyList<UnitSeedDefinition> Units =
    [
        new("ea", "EA", UnitOfMeasureDimension.Count, true, 1m, 0, true, 1),
        new("kg", "KG", UnitOfMeasureDimension.Mass, true, 1m, 3, true, 2),
        new("g", "G", UnitOfMeasureDimension.Mass, false, 0.001m, 3, true, 3),
        new("l", "L", UnitOfMeasureDimension.Volume, true, 1m, 3, true, 4),
        new("ml", "ML", UnitOfMeasureDimension.Volume, false, 0.001m, 3, true, 5),
        new("m", "M", UnitOfMeasureDimension.Length, true, 1m, 3, true, 6),
        new("cm", "CM", UnitOfMeasureDimension.Length, false, 0.01m, 3, true, 7),
        new("mm", "MM", UnitOfMeasureDimension.Length, false, 0.001m, 3, true, 8),
        new("box", "BOX", UnitOfMeasureDimension.Count, false, 1m, 0, true, 9),
        new("pallet", "PALLET", UnitOfMeasureDimension.Count, false, 1m, 0, true, 10)
    ];

    public static readonly IReadOnlyList<UnitTranslationSeed> Translations =
    [
        new("ea", "es-CL", "Unidad", "un"),
        new("kg", "es-CL", "Kilogramo", "kg"),
        new("g", "es-CL", "Gramo", "g"),
        new("l", "es-CL", "Litro", "L"),
        new("ml", "es-CL", "Mililitro", "mL"),
        new("m", "es-CL", "Metro", "m"),
        new("cm", "es-CL", "Centímetro", "cm"),
        new("mm", "es-CL", "Milímetro", "mm"),
        new("box", "es-CL", "Caja", "caja"),
        new("pallet", "es-CL", "Pallet", "pallet")
    ];

    public static readonly IReadOnlyList<UnitExternalMappingSeed> ExternalMappings =
    [
        new("ea", UnitOfMeasureMappingScheme.Unece, "EA"),
        new("kg", UnitOfMeasureMappingScheme.Unece, "KGM"),
        new("l", UnitOfMeasureMappingScheme.Unece, "LTR")
    ];

    public static readonly IReadOnlyList<string> DefaultCompanyUnits =
    [
        "ea",
        "kg",
        "g",
        "l",
        "ml",
        "m",
        "cm",
        "mm",
        "box",
        "pallet"
    ];

    public static readonly IReadOnlyDictionary<UnitOfMeasureDimension, string> DefaultCompanyDimensionUnits =
        new Dictionary<UnitOfMeasureDimension, string>
        {
            [UnitOfMeasureDimension.Count] = "ea",
            [UnitOfMeasureDimension.Mass] = "kg",
            [UnitOfMeasureDimension.Volume] = "l",
            [UnitOfMeasureDimension.Length] = "m"
        };

    public sealed record UnitSeedDefinition(
        string CanonicalCode,
        string DisplayCode,
        UnitOfMeasureDimension Dimension,
        bool IsBaseUnit,
        decimal FactorToBase,
        int PrecisionScale,
        bool IsActive,
        int SortOrder);

    public sealed record UnitTranslationSeed(string CanonicalCode, string Culture, string Name, string? Symbol);

    public sealed record UnitExternalMappingSeed(string CanonicalCode, UnitOfMeasureMappingScheme Scheme, string Code);
}
