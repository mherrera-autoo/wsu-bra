using ERP.Shared.Domain;

namespace ERP.Modules.MasterData.Domain;

public enum CustomerType
{
    Unknown = 0,
    Pharmacy = 1,
    MedicalCenter = 2,
    Clinic = 3,
    Other = 4
}

public sealed class Customer : CompanyEntity
{
    public string Name { get; private set; } = null!;
    public string? TaxId { get; private set; }
    public CustomerType Type { get; private set; } = CustomerType.Unknown;

    private Customer() { }

    public static Customer Create(long companyId, string name, string? taxId = null, CustomerType type = CustomerType.Unknown)
        => new() { CompanyId = companyId, Name = name.Trim(), TaxId = taxId?.Trim(), Type = type };

    public void Update(string name, string? taxId, CustomerType type = CustomerType.Unknown)
    {
        Name = name.Trim();
        TaxId = taxId?.Trim();
        Type = type;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetType(CustomerType type)
    {
        Type = type;
        UpdatedAt = DateTime.UtcNow;
    }
}
