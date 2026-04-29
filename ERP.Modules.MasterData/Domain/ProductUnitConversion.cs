using ERP.Shared.Domain;

namespace ERP.Modules.MasterData.Domain;

public sealed class ProductUnitConversion : Entity
{
    public long ProductId { get; private set; }
    public long FromUnitOfMeasureId { get; private set; }
    public UnitOfMeasure? FromUnitOfMeasure { get; private set; }
    public long ToUnitOfMeasureId { get; private set; }
    public UnitOfMeasure? ToUnitOfMeasure { get; private set; }
    public decimal Factor { get; private set; }
    public UnitConversionRoundingMode? RoundingMode { get; private set; }

    private ProductUnitConversion() { }

    public static ProductUnitConversion Create(
        long productId,
        long fromUnitOfMeasureId,
        long toUnitOfMeasureId,
        decimal factor,
        UnitConversionRoundingMode? roundingMode)
        => new()
        {
            ProductId = productId,
            FromUnitOfMeasureId = fromUnitOfMeasureId,
            ToUnitOfMeasureId = toUnitOfMeasureId,
            Factor = factor,
            RoundingMode = roundingMode
        };
}
