using ERP.Shared.Domain;
using ERP.Shared.Domain.ValueObjects;

namespace ERP.Modules.Billing.Domain;

public enum InvoiceStatus { Draft = 1, Issued = 2, Cancelled = 3 }

public sealed class Invoice : CompanyEntity
{
    public long Number { get; private set; }
    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;
    public long CustomerId { get; private set; }
    public DateTime? IssueDate { get; private set; }
    public string? Currency { get; private set; }
    public List<InvoiceLine> Lines { get; private set; } = new();

    private Invoice() { }

    public static Invoice Create(long companyId, long customerId)
        => new() { CompanyId = companyId, CustomerId = customerId };

    public void AddLine(long productId, decimal qty, Money unitPrice)
    {
        if (Status != InvoiceStatus.Draft) throw new InvalidOperationException("Only Draft invoices can be edited.");
        if (Currency is null)
        {
            Currency = unitPrice.Currency;
        }
        else if (!string.Equals(Currency, unitPrice.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("All invoice lines must use the same currency.");
        }

        Lines.Add(InvoiceLine.Create(CompanyId, Id, productId, qty, unitPrice));
    }

    public Money GetTotal()
    {
        if (!Lines.Any())
        {
            return new Money(0m, Currency ?? string.Empty);
        }

        var total = Lines.Sum(line => line.Qty * line.UnitPrice.Amount);
        return new Money(total, Currency ?? Lines[0].UnitPrice.Currency);
    }

    public void Issue(DateTime issuedAt)
    {
        Issue(0, issuedAt);
    }

    public void Issue(long number, DateTime issuedAt)
    {
        if (!Lines.Any()) throw new InvalidOperationException("Cannot issue invoice without lines.");
        Number = number;
        Status = InvoiceStatus.Issued;
        IssueDate = issuedAt;
    }

    public void Cancel()
    {
        if (Status != InvoiceStatus.Issued) throw new InvalidOperationException("Only Issued invoices can be cancelled.");
        Status = InvoiceStatus.Cancelled;
    }
}

public sealed class InvoiceLine : CompanyEntity
{
    public long InvoiceId { get; private set; }
    public long ProductId { get; private set; }
    public decimal Qty { get; private set; }
    public Money UnitPrice { get; private set; }

    private InvoiceLine() { }

    public static InvoiceLine Create(long companyId, long invoiceId, long productId, decimal qty, Money unitPrice)
    {
        if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
        return new()
        {
            CompanyId = companyId,
            InvoiceId = invoiceId,
            ProductId = productId,
            Qty = qty,
            UnitPrice = unitPrice
        };
    }
}
