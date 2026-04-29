using ERP.Shared.Domain;

namespace ERP.Modules.Accounting.Domain;

public enum AccountingRuleTrigger
{
    DteReceived = 1,
    PaymentReceived = 2,
    PaymentSent = 3,
    Adjustment = 4,
    BankStatementMatch = 5
}

public sealed class AccountingAutomationRule : CompanyEntity
{
    public string Name { get; private set; } = string.Empty;
    public AccountingRuleTrigger Trigger { get; private set; }
    public DteDocumentType? DteDocumentType { get; private set; }
    public string? SourceModule { get; private set; }
    public string? SourceDocumentType { get; private set; }
    public string? CounterpartyTaxId { get; private set; }
    public long DebitAccountId { get; private set; }
    public long CreditAccountId { get; private set; }
    public string? TaxCode { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool ProposedBySystem { get; private set; }

    private AccountingAutomationRule() { }

    public static AccountingAutomationRule CreateManual(
        long companyId,
        string name,
        AccountingRuleTrigger trigger,
        DteDocumentType? dteDocumentType,
        string? sourceModule,
        string? sourceDocumentType,
        string? counterpartyTaxId,
        long debitAccountId,
        long creditAccountId,
        string? taxCode)
        => new AccountingAutomationRule
        {
            CompanyId = companyId,
            Name = name.Trim(),
            Trigger = trigger,
            DteDocumentType = dteDocumentType,
            SourceModule = sourceModule,
            SourceDocumentType = sourceDocumentType,
            CounterpartyTaxId = string.IsNullOrWhiteSpace(counterpartyTaxId) ? null : counterpartyTaxId.Trim(),
            DebitAccountId = debitAccountId,
            CreditAccountId = creditAccountId,
            TaxCode = taxCode,
            ProposedBySystem = false
        };

    public static AccountingAutomationRule CreateProposed(
        long companyId,
        string name,
        AccountingRuleTrigger trigger,
        DteDocumentType? dteDocumentType,
        string? sourceModule,
        string? sourceDocumentType,
        string? counterpartyTaxId,
        long debitAccountId,
        long creditAccountId,
        string? taxCode)
    {
        var rule = CreateManual(
            companyId,
            name,
            trigger,
            dteDocumentType,
            sourceModule,
            sourceDocumentType,
            counterpartyTaxId,
            debitAccountId,
            creditAccountId,
            taxCode);
        rule.ProposedBySystem = true;
        return rule;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }
}
