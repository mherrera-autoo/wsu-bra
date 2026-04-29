namespace ERP.Api.Contracts.Cash;

public sealed record CreateBankTransactionRequest(
    long CompanyId,
    long BankStatementId,
    decimal Amount,
    string Currency,
    int Kind,
    DateTime? TransactionDate,
    string? Description,
    string? Reference);
