using ERP.Shared.Domain;

namespace ERP.Modules.Accounting.Domain;

public enum TaxBookType
{
    Sales = 1,
    Purchases = 2
}

public sealed class TaxBookEntry : CompanyEntity
{
    public int Year { get; private set; }
    public int Month { get; private set; }
    public TaxBookType BookType { get; private set; }
    public long DteDocumentId { get; private set; }
    public string Folio { get; private set; } = string.Empty;
    public DteDocumentType DocumentType { get; private set; }
    public DateTime IssueDate { get; private set; }
    public string CounterpartyTaxId { get; private set; } = string.Empty;
    public decimal NetAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string CurrencyCode { get; private set; } = string.Empty;

    private TaxBookEntry() { }

    public static TaxBookEntry Create(
        long companyId,
        int year,
        int month,
        TaxBookType bookType,
        DteDocument dte)
    {
        if (dte is null) throw new ArgumentNullException(nameof(dte));
        if (year <= 0) throw new ArgumentOutOfRangeException(nameof(year));
        if (month is < 1 or > 12) throw new ArgumentOutOfRangeException(nameof(month));

        return new TaxBookEntry
        {
            CompanyId = companyId,
            Year = year,
            Month = month,
            BookType = bookType,
            DteDocumentId = dte.Id,
            Folio = dte.Folio,
            DocumentType = dte.DocumentType,
            IssueDate = dte.IssueDate,
            CounterpartyTaxId = dte.CounterpartyTaxId,
            NetAmount = dte.NetAmount,
            TaxAmount = dte.TaxAmount,
            TotalAmount = dte.TotalAmount,
            CurrencyCode = dte.CurrencyCode
        };
    }
}
