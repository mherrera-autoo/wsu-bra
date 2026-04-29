using ERP.Shared.Domain;
using ERP.Shared.Domain.ValueObjects;

namespace ERP.Modules.Sales.Domain;

public enum SalesDocumentKind { Quote = 1, Order = 2, Invoice = 3, CreditNote = 4 }
public enum SalesDocumentStatus { Draft = 1, Approved = 2, Sent = 3, Cancelled = 4 }

public sealed class SalesDocument : CompanyEntity
{
    public SalesDocumentKind Kind { get; private set; }
    public SalesDocumentStatus Status { get; private set; } = SalesDocumentStatus.Draft;
    public long CustomerId { get; private set; }
    public List<SalesDocumentLine> Lines { get; private set; } = new();

    private SalesDocument() { }

    public static SalesDocument Create(long companyId, SalesDocumentKind kind, long customerId)
        => new() { CompanyId = companyId, Kind = kind, CustomerId = customerId };

    public void AddLine(long productId, decimal qty, Money unitPrice, long? taxGroupId, Money taxAmount)
    {
        if (Status != SalesDocumentStatus.Draft) throw new InvalidOperationException("Only Draft documents can be edited.");
        Lines.Add(SalesDocumentLine.Create(CompanyId, Id, productId, qty, unitPrice, taxGroupId, taxAmount));
    }

    public void Approve()
    {
        if (!Lines.Any()) throw new InvalidOperationException("Cannot approve without lines.");
        Status = SalesDocumentStatus.Approved;
    }

    public (decimal totalAmount, string currency) CalculateTotal()
    {
        if (!Lines.Any()) throw new InvalidOperationException("Cannot total a document without lines.");
        var currency = Lines[0].UnitPrice.Currency;
        if (Lines.Any(line => line.UnitPrice.Currency != currency))
        {
            throw new InvalidOperationException("All lines must use the same currency.");
        }

        var total = Lines.Sum(line => line.Qty * line.UnitPrice.Amount);
        return (total, currency);
    }
}

public sealed class SalesDocumentLine : CompanyEntity
{
    public long SalesDocumentId { get; private set; }
    public long ProductId { get; private set; }
    public decimal Qty { get; private set; }
    public Money UnitPrice { get; private set; }
    public long? TaxGroupId { get; private set; }
    public Money NetAmount { get; private set; }
    public Money TaxAmount { get; private set; }
    public Money TotalAmount { get; private set; }

    private SalesDocumentLine() { }

    public static SalesDocumentLine Create(long companyId, long docId, long productId, decimal qty, Money unitPrice, long? taxGroupId, Money taxAmount)
    {
        if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
        var netAmount = new Money(qty * unitPrice.Amount, unitPrice.Currency);
        if (taxAmount.Currency != unitPrice.Currency) throw new InvalidOperationException("Tax amount currency mismatch.");
        var totalAmount = new Money(netAmount.Amount + taxAmount.Amount, unitPrice.Currency);
        return new()
        {
            CompanyId = companyId,
            SalesDocumentId = docId,
            ProductId = productId,
            Qty = qty,
            UnitPrice = unitPrice,
            TaxGroupId = taxGroupId,
            NetAmount = netAmount,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount
        };
    }
}
