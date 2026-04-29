namespace ERP.Api.Contracts.Billing;

public sealed class CancelDebitNoteRequest
{
    public long ReceivableAccountId { get; set; }
    public long RevenueAccountId { get; set; }
}
