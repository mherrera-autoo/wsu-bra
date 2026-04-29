using ERP.Shared.Domain;

namespace ERP.Modules.MasterData.Domain;

public sealed class UnitOfMeasureTranslation : Entity
{
    public long UnitOfMeasureId { get; private set; }
    public UnitOfMeasure? UnitOfMeasure { get; private set; }
    public string Culture { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Symbol { get; private set; }

    private UnitOfMeasureTranslation() { }

    public static UnitOfMeasureTranslation Create(long unitOfMeasureId, string culture, string name, string? symbol)
        => new()
        {
            UnitOfMeasureId = unitOfMeasureId,
            Culture = culture.Trim(),
            Name = name.Trim(),
            Symbol = string.IsNullOrWhiteSpace(symbol) ? null : symbol.Trim()
        };

    public void Update(string name, string? symbol)
    {
        Name = name.Trim();
        Symbol = string.IsNullOrWhiteSpace(symbol) ? null : symbol.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
