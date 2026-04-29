namespace ERP.Api.Contracts.Billing;

public sealed class CancelCreditNoteRequest
{
    public long ReceivableAccountId { get; set; }
    public long RevenueAccountId { get; set; }
}
