namespace ERP.Api.Contracts.Billing;

public sealed record IssueInvoiceRequest(
    long CustomerId,
    long ReceivableAccountId,
    long RevenueAccountId,
    IReadOnlyList<IssueInvoiceLineRequest> Lines);

public sealed record IssueInvoiceLineRequest(
    long ProductId,
    decimal Qty,
    decimal UnitPriceAmount,
    string UnitPriceCurrency);
