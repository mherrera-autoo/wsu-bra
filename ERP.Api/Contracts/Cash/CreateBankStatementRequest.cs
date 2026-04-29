namespace ERP.Api.Contracts.Cash;

public sealed record CreateBankStatementRequest(
    long CompanyId,
    long BankAccountId,
    DateTime StatementDate,
    decimal StartingBalance,
    decimal EndingBalance,
    IReadOnlyList<CreateBankStatementTransactionRequest> Transactions);

public sealed record CreateBankStatementTransactionRequest(
    DateTime TransactionDate,
    decimal Amount,
    string Description,
    string? Reference);
