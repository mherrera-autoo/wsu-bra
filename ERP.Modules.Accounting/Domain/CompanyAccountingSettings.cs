namespace ERP.Modules.Accounting.Domain;

public sealed class CompanyAccountingSettings
{
    public long CompanyId { get; private set; }
    public string BaseCurrencyCode { get; private set; } = null!;
    public long? AccountsReceivableAccountId { get; private set; }
    public long? AccountsPayableAccountId { get; private set; }
    public long? SalesRevenueAccountId { get; private set; }
    public long? VatDebitAccountId { get; private set; }
    public long? VatCreditAccountId { get; private set; }
    public long? CashAccountId { get; private set; }
    public long? BankAccountId { get; private set; }

    private CompanyAccountingSettings() { }

    public static CompanyAccountingSettings Create(long companyId, string baseCurrencyCode)
    {
        if (string.IsNullOrWhiteSpace(baseCurrencyCode))
        {
            throw new ArgumentException("Base currency code is required.", nameof(baseCurrencyCode));
        }

        return new CompanyAccountingSettings
        {
            CompanyId = companyId,
            BaseCurrencyCode = baseCurrencyCode.Trim().ToUpperInvariant()
        };
    }
}
