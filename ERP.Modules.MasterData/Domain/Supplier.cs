using ERP.Shared.Domain;

namespace ERP.Modules.MasterData.Domain;

public sealed class Supplier : CompanyEntity
{
    public string Name { get; private set; } = null!;
    public string? TaxId { get; private set; }
    public long? CountryId { get; private set; }
    public long? DefaultCurrencyId { get; private set; }
    public bool IsActive { get; private set; } = true;

    public Country? Country { get; private set; }
    public Currency? DefaultCurrency { get; private set; }
    public ICollection<ProductSupplier> ProductSuppliers { get; private set; } = new List<ProductSupplier>();

    private Supplier() { }

    public static Supplier Create(long companyId, string name, string? taxId = null, string? country = null, string? currency = null)
        => new()
        {
            CompanyId = companyId,
            Name = name.Trim(),
            TaxId = taxId?.Trim(),
            CountryId = null,
            DefaultCurrencyId = null,
            IsActive = true
        };

    public static Supplier CreateWithIds(long companyId, string name, string? taxId = null, long? countryId = null, long? defaultCurrencyId = null)
        => new()
        {
            CompanyId = companyId,
            Name = name.Trim(),
            TaxId = taxId?.Trim(),
            CountryId = countryId,
            DefaultCurrencyId = defaultCurrencyId,
            IsActive = true
        };

    public void Update(string name, string? taxId, string? country, string? currency, bool isActive)
    {
        Name = name.Trim();
        TaxId = taxId?.Trim();
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateWithIds(string name, string? taxId, long? countryId, long? defaultCurrencyId, bool isActive)
    {
        Name = name.Trim();
        TaxId = taxId?.Trim();
        CountryId = countryId;
        DefaultCurrencyId = defaultCurrencyId;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
