using ERP.Shared.Domain;

namespace ERP.Modules.Sales.Domain;

public enum SalesQuoteStatus
{
    Draft = 1,
    Submitted = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 5
}

public sealed class SalesQuote : CompanyEntity
{
    private readonly List<SalesQuoteLine> _lines = new();

    private SalesQuote() { }

    public string QuoteNumber { get; private set; } = string.Empty;
    public SalesQuoteStatus Status { get; private set; } = SalesQuoteStatus.Draft;
    public long? CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public string? CurrencyCode { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal TaxTotal { get; private set; }
    public decimal Total { get; private set; }
    public bool IsActive { get; private set; } = true;
    public long CreatedByUserId { get; private set; }
    public long UpdatedByUserId { get; private set; }
    public IReadOnlyList<SalesQuoteLine> Lines => _lines;

    public static SalesQuote Create(
        long companyId,
        string quoteNumber,
        string customerName,
        string? notes,
        string? currencyCode,
        long createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(quoteNumber)) throw new ArgumentException("Quote number is required.", nameof(quoteNumber));
        if (string.IsNullOrWhiteSpace(customerName)) throw new ArgumentException("Customer name is required.", nameof(customerName));

        var now = DateTime.UtcNow;
        return new SalesQuote
        {
            CompanyId = companyId,
            QuoteNumber = quoteNumber.Trim(),
            CustomerName = customerName.Trim(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            CurrencyCode = string.IsNullOrWhiteSpace(currencyCode) ? null : currencyCode.Trim().ToUpperInvariant(),
            CreatedByUserId = createdByUserId,
            UpdatedByUserId = createdByUserId,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void SetCustomerId(long? customerId)
    {
        CustomerId = customerId;
    }

    public void AddLine(
        int lineNumber,
        long? productId,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal? taxRate)
    {
        if (Status != SalesQuoteStatus.Draft)
        {
            throw new InvalidOperationException("Only draft quotes can be edited.");
        }

        _lines.Add(SalesQuoteLine.Create(CompanyId, Id, lineNumber, productId, description, quantity, unitPrice, taxRate));
        RecalculateTotals();
    }

    public void Submit(long updatedByUserId)
    {
        if (Status != SalesQuoteStatus.Draft)
        {
            throw new InvalidOperationException("Only draft quotes can be submitted.");
        }

        Status = SalesQuoteStatus.Submitted;
        Touch(updatedByUserId);
    }

    public void Approve(long updatedByUserId)
    {
        if (Status != SalesQuoteStatus.Submitted)
        {
            throw new InvalidOperationException("Only submitted quotes can be approved.");
        }

        Status = SalesQuoteStatus.Approved;
        Touch(updatedByUserId);
    }

    public void Reject(long updatedByUserId)
    {
        if (Status != SalesQuoteStatus.Submitted)
        {
            throw new InvalidOperationException("Only submitted quotes can be rejected.");
        }

        Status = SalesQuoteStatus.Rejected;
        Touch(updatedByUserId);
    }

    private void RecalculateTotals()
    {
        var subtotal = _lines.Sum(CalculateNetAmount);
        var taxTotal = _lines.Sum(CalculateTaxAmount);
        Subtotal = RoundAmount(subtotal);
        TaxTotal = RoundAmount(taxTotal);
        Total = RoundAmount(Subtotal + TaxTotal);
    }

    private void Touch(long updatedByUserId)
    {
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    private static decimal RoundAmount(decimal amount)
        => Math.Round(amount, 2, MidpointRounding.AwayFromZero);

    private static decimal CalculateNetAmount(SalesQuoteLine line)
        => line.Quantity * line.UnitPrice;

    private static decimal CalculateTaxAmount(SalesQuoteLine line)
        => line.TaxRate is null ? 0m : CalculateNetAmount(line) * (line.TaxRate.Value / 100m);
}

public sealed class SalesQuoteLine : CompanyEntity
{
    private SalesQuoteLine() { }

    public long QuoteId { get; private set; }
    public int LineNumber { get; private set; }
    public long? ProductId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal { get; private set; }
    public decimal? TaxRate { get; private set; }
    public bool IsActive { get; private set; } = true;

    public static SalesQuoteLine Create(
        long companyId,
        long quoteId,
        int lineNumber,
        long? productId,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal? taxRate)
    {
        if (lineNumber <= 0) throw new ArgumentOutOfRangeException(nameof(lineNumber));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Line description is required.", nameof(description));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (unitPrice < 0) throw new ArgumentOutOfRangeException(nameof(unitPrice));
        if (taxRate is < 0) throw new ArgumentOutOfRangeException(nameof(taxRate));

        var netAmount = quantity * unitPrice;
        var taxAmount = taxRate is null ? 0m : netAmount * (taxRate.Value / 100m);
        var lineTotal = netAmount + taxAmount;

        return new SalesQuoteLine
        {
            CompanyId = companyId,
            QuoteId = quoteId,
            LineNumber = lineNumber,
            ProductId = productId,
            Description = description.Trim(),
            Quantity = quantity,
            UnitPrice = unitPrice,
            TaxRate = taxRate,
            LineTotal = RoundAmount(lineTotal)
        };
    }

    private static decimal RoundAmount(decimal amount)
        => Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}
