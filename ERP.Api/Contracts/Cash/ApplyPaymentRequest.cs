namespace ERP.Api.Contracts.Cash;

public sealed record ApplyPaymentRequest(
    long CompanyId,
    long PurchaseOrderId,
    decimal Amount,
    long BankAccountId,
    long AccountsPayableAccountId);
