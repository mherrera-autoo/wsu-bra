namespace ERP.Api.Contracts.Billing;

public sealed class IssueDebitNoteRequest
{
    public long InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    public long ReceivableAccountId { get; set; }
    public long RevenueAccountId { get; set; }
}
