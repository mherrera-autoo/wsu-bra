using ERP.Shared.Domain;

namespace ERP.Modules.MasterData.Domain;

public sealed class Country : Entity
{
    public string Iso2 { get; private set; } = null!;
    public string Iso3 { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? NumericCode { get; private set; }
    public string? PhonePrefix { get; private set; }
    public string? CurrencyCode { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Country() { }

    public static Country Create(
        string iso2,
        string iso3,
        string name,
        string? numericCode,
        string? phonePrefix,
        string? currencyCode,
        bool isActive)
    {
        return new Country
        {
            Iso2 = iso2.Trim().ToUpperInvariant(),
            Iso3 = iso3.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            NumericCode = string.IsNullOrWhiteSpace(numericCode) ? null : numericCode.Trim(),
            PhonePrefix = string.IsNullOrWhiteSpace(phonePrefix) ? null : phonePrefix.Trim(),
            CurrencyCode = string.IsNullOrWhiteSpace(currencyCode) ? null : currencyCode.Trim().ToUpperInvariant(),
            IsActive = isActive
        };
    }

    public void Update(
        string name,
        string? numericCode,
        string? phonePrefix,
        string? currencyCode,
        bool isActive)
    {
        Name = name.Trim();
        NumericCode = string.IsNullOrWhiteSpace(numericCode) ? null : numericCode.Trim();
        PhonePrefix = string.IsNullOrWhiteSpace(phonePrefix) ? null : phonePrefix.Trim();
        CurrencyCode = string.IsNullOrWhiteSpace(currencyCode) ? null : currencyCode.Trim().ToUpperInvariant();
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }
}
