using ERP.Shared.Domain;
using ERP.Shared.Domain.ValueObjects;

namespace ERP.Modules.Billing.Domain;

public sealed class DebitNote : CompanyEntity
{
    public long InvoiceId { get; private set; }
    public long Number { get; private set; }
    public BillingDocumentStatus Status { get; private set; } = BillingDocumentStatus.Draft;
    public Money Amount { get; private set; }
    public DateTime? IssuedAt { get; private set; }

    private DebitNote() { }

    public static DebitNote Create(long companyId, long invoiceId, Money amount)
        => new()
        {
            CompanyId = companyId,
            InvoiceId = invoiceId,
            Amount = amount
        };

    public void Issue(long number, DateTime issuedAt)
    {
        if (Status != BillingDocumentStatus.Draft)
        {
            throw new InvalidOperationException("Only Draft debit notes can be issued.");
        }

        Number = number;
        IssuedAt = issuedAt;
        Status = BillingDocumentStatus.Issued;
    }

    public void Cancel()
    {
        if (Status != BillingDocumentStatus.Issued)
        {
            throw new InvalidOperationException("Only Issued debit notes can be cancelled.");
        }

        Status = BillingDocumentStatus.Cancelled;
    }
}
