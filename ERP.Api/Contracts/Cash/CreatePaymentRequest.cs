namespace ERP.Api.Contracts.Cash;

public sealed record CreatePaymentRequest(
    long CompanyId,
    long SupplierId,
    long BankAccountId,
    decimal Amount,
    string Currency,
    DateTime PaidAt,
    long PayableAccountId,
    long BankLedgerAccountId,
    long? PurchaseOrderId,
    string? Reference);
