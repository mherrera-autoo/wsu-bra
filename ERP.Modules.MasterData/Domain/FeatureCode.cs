using ERP.Shared.Application;

namespace ERP.Modules.MasterData.Domain;

public sealed class FeatureCode : IEquatable<FeatureCode>
{
    public static readonly FeatureCode PharmaceuticalDrogueria = new(
        FeatureKeys.PharmaceuticalDrogueria,
        new Guid("bf214755-216c-4053-99d4-f7b5b87cb4fd"),
        "Drogueria",
        "Habilita capacidades base del vertical drogueria.");

    public static readonly FeatureCode PharmaceuticalBase = new(
        FeatureKeys.PharmaceuticalBase,
        new Guid("b67457d8-1675-45a8-95f8-63cc4f4f6644"),
        "Pharmacy Base",
        "Desbloquea funciones base del modulo de farmacia.");

    public static readonly FeatureCode PharmaceuticalDispensing = new(
        FeatureKeys.PharmaceuticalDispensing,
        new Guid("46cc95f6-be87-4f22-a80f-dfa194fd8ba1"),
        "Pharmacy Dispensing",
        "Permite el flujo de dispensacion farmacologica.");

    public static readonly FeatureCode PharmaceuticalComplianceIsp = new(
        FeatureKeys.PharmaceuticalComplianceIsp,
        new Guid("31f4f6a8-46fc-4fcf-a951-bd79832f1af0"),
        "Pharmacy Compliance ISP",
        "Activa controles y reportes regulatorios ISP.");

    public static readonly FeatureCode WmsWarehouse = new(
        FeatureKeys.WmsWarehouse,
        new Guid("de7f5ebb-bb90-4b7f-a750-002b6f9f30a3"),
        "Warehouse WMS",
        "Habilita el modulo de gestion de bodega (WMS).");

    public static readonly FeatureCode Rfid = new(
        FeatureKeys.Rfid,
        new Guid("9d8f7bf7-8d22-4e4c-9d45-95311ad76e72"),
        "Rfid",
        "Habilita las capacidades del modulo RFID.");

    private static readonly IReadOnlyList<FeatureCode> Items =
    [
        PharmaceuticalBase,
        PharmaceuticalDrogueria,
        PharmaceuticalDispensing,
        PharmaceuticalComplianceIsp,
        WmsWarehouse,
        Rfid
    ];

    private static readonly IReadOnlyDictionary<string, FeatureCode> ByCode =
        Items.ToDictionary(item => item.Code, StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlyDictionary<Guid, FeatureCode> ByPublicId =
        Items.ToDictionary(item => item.PublicId);

    private FeatureCode(
        string code,
        Guid publicId,
        string name,
        string? description,
        bool defaultIsActive = true)
    {
        Code = code;
        PublicId = publicId;
        Name = name;
        Description = description;
        DefaultIsActive = defaultIsActive;
    }

    public string Code { get; }
    public Guid PublicId { get; }
    public string Name { get; }
    public string? Description { get; }
    public bool DefaultIsActive { get; }

    public static IReadOnlyCollection<FeatureCode> Supported => Items;

    public static FeatureCode FromCode(string code)
    {
        if (TryFromCode(code, out var featureCode))
        {
            return featureCode;
        }

        throw new ArgumentOutOfRangeException(nameof(code), code, "Unsupported feature code.");
    }

    public static FeatureCode FromPublicId(Guid publicId)
    {
        if (TryFromPublicId(publicId, out var featureCode))
        {
            return featureCode;
        }

        throw new ArgumentOutOfRangeException(nameof(publicId), publicId, "Unsupported feature public id.");
    }

    public static bool TryFromCode(string? code, out FeatureCode featureCode)
    {
        if (!string.IsNullOrWhiteSpace(code) && ByCode.TryGetValue(code, out var value))
        {
            featureCode = value;
            return true;
        }

        featureCode = null!;
        return false;
    }

    public static bool TryFromPublicId(Guid publicId, out FeatureCode featureCode)
    {
        if (publicId != Guid.Empty && ByPublicId.TryGetValue(publicId, out var value))
        {
            featureCode = value;
            return true;
        }

        featureCode = null!;
        return false;
    }

    public bool Equals(FeatureCode? other)
    {
        return other is not null && PublicId == other.PublicId;
    }

    public override bool Equals(object? obj)
    {
        return obj is FeatureCode other && Equals(other);
    }

    public override int GetHashCode()
    {
        return PublicId.GetHashCode();
    }

    public override string ToString()
    {
        return Code;
    }
}
