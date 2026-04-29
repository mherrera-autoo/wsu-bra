using ERP.Shared.Domain;

namespace ERP.Modules.MasterData.Domain;

public sealed class CompanyCurrency : Entity
{
    public long CompanyId { get; private set; }
    public long CurrencyId { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }

    public Company Company { get; private set; } = null!;
    public Currency Currency { get; private set; } = null!;

    private CompanyCurrency() { }

    public static CompanyCurrency Create(
        long companyId,
        long currencyId,
        bool isDefault = false,
        bool isActive = true)
    {
        return new CompanyCurrency
        {
            CompanyId = companyId,
            CurrencyId = currencyId,
            IsDefault = isDefault,
            IsActive = isActive
        };
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnsetDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}