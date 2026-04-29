using ERP.Shared.Domain;
using ERP.Shared.Domain.ValueObjects;

namespace ERP.Modules.Billing.Domain;

public sealed class CreditNote : CompanyEntity
{
    public long InvoiceId { get; private set; }
    public long Number { get; private set; }
    public BillingDocumentStatus Status { get; private set; } = BillingDocumentStatus.Draft;
    public DateTime? IssuedAt { get; private set; }
    public string Reason { get; private set; } = null!;
    public Money Amount { get; private set; }

    private CreditNote() { }

    public static CreditNote Create(long companyId, long invoiceId, Money amount, string reason)
        => new()
        {
            CompanyId = companyId,
            InvoiceId = invoiceId,
            Amount = amount,
            Reason = reason.Trim()
        };

    public void Issue(long number, DateTime issuedAt)
    {
        if (Status != BillingDocumentStatus.Draft)
        {
            throw new InvalidOperationException("Only Draft credit notes can be issued.");
        }

        Number = number;
        IssuedAt = issuedAt;
        Status = BillingDocumentStatus.Issued;
    }

    public void Cancel()
    {
        if (Status != BillingDocumentStatus.Issued)
        {
            throw new InvalidOperationException("Only Issued credit notes can be cancelled.");
        }

        Status = BillingDocumentStatus.Cancelled;
    }
}
