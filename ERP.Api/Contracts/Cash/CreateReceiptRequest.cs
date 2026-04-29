namespace ERP.Api.Contracts.Cash;

public sealed record CreateReceiptRequest(
    long CompanyId,
    long CustomerId,
    long? SalesDocumentId,
    long BankAccountId,
    decimal Amount,
    string Currency,
    DateTime ReceivedAt,
    long ReceivableAccountId,
    long BankLedgerAccountId,
    string? Reference);
