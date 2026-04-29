using ERP.Shared.Domain;
using ERP.Shared.Domain.ValueObjects;

namespace ERP.Modules.Purchasing.Domain;

public enum PurchaseOrderStatus { Draft = 1, Approved = 2, Sent = 3, PartiallyReceived = 4, Closed = 5, Cancelled = 6 }

public sealed class PurchaseOrder : CompanyEntity
{
    public long SupplierId { get; private set; }
    public PurchaseOrderStatus Status { get; private set; } = PurchaseOrderStatus.Draft;
    public string Currency { get; private set; } = "CLP";
    public List<PurchaseOrderLine> Lines { get; private set; } = new();

    private PurchaseOrder() { }

    public static PurchaseOrder Create(long companyId, long supplierId, string currency = "CLP")
        => new() { CompanyId = companyId, SupplierId = supplierId, Currency = currency.Trim() };

    public void AddLine(long productId, decimal qty, Money unitPriceRef, long? taxGroupId, Money taxAmount)
    {
        if (Status != PurchaseOrderStatus.Draft) throw new InvalidOperationException("Can only edit Draft POs.");
        Lines.Add(PurchaseOrderLine.Create(CompanyId, Id, productId, qty, unitPriceRef, taxGroupId, taxAmount));
    }
}

public sealed class PurchaseOrderLine : CompanyEntity
{
    public long PurchaseOrderId { get; private set; }
    public long ProductId { get; private set; }
    public decimal OrderedQty { get; private set; }
    public Money UnitPriceRef { get; private set; } = null!;
    public long? TaxGroupId { get; private set; }
    public Money NetAmount { get; private set; } = null!;
    public Money TaxAmount { get; private set; } = null!;
    public Money TotalAmount { get; private set; } = null!;

    private PurchaseOrderLine() { }

    public static PurchaseOrderLine Create(long companyId, long poId, long productId, decimal orderedQty, Money unitPriceRef, long? taxGroupId, Money taxAmount)
    {
        if (orderedQty <= 0) throw new ArgumentOutOfRangeException(nameof(orderedQty));
        if (unitPriceRef.Currency != taxAmount.Currency) throw new InvalidOperationException("Tax currency must match unit price currency.");
        var netAmount = new Money(orderedQty * unitPriceRef.Amount, unitPriceRef.Currency);
        var totalAmount = new Money(netAmount.Amount + taxAmount.Amount, unitPriceRef.Currency);
        return new()
        {
            CompanyId = companyId,
            PurchaseOrderId = poId,
            ProductId = productId,
            OrderedQty = orderedQty,
            UnitPriceRef = unitPriceRef,
            TaxGroupId = taxGroupId,
            NetAmount = netAmount,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount
        };
    }
}
