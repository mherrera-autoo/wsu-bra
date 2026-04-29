using ERP.Shared.Domain;

namespace ERP.Modules.MasterData.Domain;

public sealed class UnitOfMeasureExternalMapping : Entity
{
    public long UnitOfMeasureId { get; private set; }
    public UnitOfMeasure? UnitOfMeasure { get; private set; }
    public UnitOfMeasureMappingScheme Scheme { get; private set; }
    public string Code { get; private set; } = null!;

    private UnitOfMeasureExternalMapping() { }

    public static UnitOfMeasureExternalMapping Create(long unitOfMeasureId, UnitOfMeasureMappingScheme scheme, string code)
        => new()
        {
            UnitOfMeasureId = unitOfMeasureId,
            Scheme = scheme,
            Code = code.Trim().ToUpperInvariant()
        };
}
