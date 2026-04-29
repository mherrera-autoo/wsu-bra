using ERP.Shared.Domain;

namespace ERP.Modules.Cash.Domain;

public sealed class BankAccount : CompanyEntity
{
    public string Name { get; private set; } = null!;
    public string BankName { get; private set; } = null!;
    public string AccountNumber { get; private set; } = null!;
    public string Currency { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;

    private BankAccount() { }

    public static BankAccount Create(long companyId, string name, string bankName, string accountNumber, string currency)
        => new()
        {
            CompanyId = companyId,
            Name = name.Trim(),
            BankName = bankName.Trim(),
            AccountNumber = accountNumber.Trim(),
            Currency = currency.Trim()
        };

    public void Deactivate() => IsActive = false;
}
