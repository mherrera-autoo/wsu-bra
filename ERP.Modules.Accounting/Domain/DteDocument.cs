using ERP.Shared.Domain;

namespace ERP.Modules.Accounting.Domain;

public enum DteDocumentType
{
    Invoice = 1,
    CreditNote = 2,
    DebitNote = 3,
    PurchaseInvoice = 4,
    PurchaseCreditNote = 5,
    ExportInvoice = 6
}

public enum DteDocumentStatus
{
    Pending = 1,
    Accepted = 2,
    Rejected = 3
}

public sealed class DteDocument : CompanyEntity
{
    public string Folio { get; private set; } = string.Empty;
    public DteDocumentType DocumentType { get; private set; }
    public DateTime IssueDate { get; private set; }
    public string CounterpartyTaxId { get; private set; } = string.Empty;
    public decimal NetAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string CurrencyCode { get; private set; } = string.Empty;
    public string Source { get; private set; } = string.Empty;
    public DteDocumentStatus Status { get; private set; } = DteDocumentStatus.Pending;
    public string? StatusReason { get; private set; }

    private DteDocument() { }

    public static DteDocument Create(
        long companyId,
        string folio,
        DteDocumentType documentType,
        DateTime issueDate,
        string counterpartyTaxId,
        decimal netAmount,
        decimal taxAmount,
        decimal totalAmount,
        string currencyCode,
        string source)
    {
        if (string.IsNullOrWhiteSpace(folio)) throw new ArgumentException("Folio is required.", nameof(folio));
        if (string.IsNullOrWhiteSpace(counterpartyTaxId))
            throw new ArgumentException("Counterparty tax id is required.", nameof(counterpartyTaxId));
        if (string.IsNullOrWhiteSpace(currencyCode))
            throw new ArgumentException("Currency code is required.", nameof(currencyCode));

        return new DteDocument
        {
            CompanyId = companyId,
            Folio = folio.Trim(),
            DocumentType = documentType,
            IssueDate = issueDate,
            CounterpartyTaxId = counterpartyTaxId.Trim(),
            NetAmount = netAmount,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            CurrencyCode = currencyCode.Trim().ToUpperInvariant(),
            Source = source.Trim()
        };
    }

    public void MarkAccepted(string? reason = null)
    {
        Status = DteDocumentStatus.Accepted;
        StatusReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkRejected(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Reason is required.", nameof(reason));
        Status = DteDocumentStatus.Rejected;
        StatusReason = reason.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
