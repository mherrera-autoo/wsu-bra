namespace ERP.Api.Contracts.Cash;

public sealed record ApplyReceiptRequest(
    long CompanyId,
    long SalesDocumentId,
    decimal Amount,
    long BankAccountId,
    long AccountsReceivableAccountId);
