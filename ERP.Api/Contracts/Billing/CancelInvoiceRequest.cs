namespace ERP.Api.Contracts.Billing;

public sealed record CancelInvoiceRequest(
    string Reason,
    long ReceivableAccountId,
    long RevenueAccountId);
