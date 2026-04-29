using ERP.Shared.Domain;

namespace ERP.Modules.MasterData.Domain;

public sealed class Currency : Entity
{
    public string Code { get; private set; } = null!;
    public int NumericCode { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Symbol { get; private set; }
    public int MinorUnits { get; private set; }
    public int Order { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Currency() { }

    public static Currency Create(
        string code,
        int numericCode,
        string name,
        string? symbol,
        int minorUnits,
        int order,
        bool isActive = true)
    {
        return new Currency
        {
            Code = code.Trim().ToUpperInvariant(),
            NumericCode = numericCode,
            Name = name.Trim(),
            Symbol = string.IsNullOrWhiteSpace(symbol) ? null : symbol.Trim(),
            MinorUnits = minorUnits,
            Order = order,
            IsActive = isActive
        };
    }

    public void Update(
        string name,
        string? symbol,
        int minorUnits,
        int order,
        bool isActive)
    {
        Name = name.Trim();
        Symbol = string.IsNullOrWhiteSpace(symbol) ? null : symbol.Trim();
        MinorUnits = minorUnits;
        Order = order;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }
}