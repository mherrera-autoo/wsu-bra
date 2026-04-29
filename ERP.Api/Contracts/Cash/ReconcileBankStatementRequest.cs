namespace ERP.Api.Contracts.Cash;

public sealed record ReconcileBankStatementRequest(
    long CompanyId,
    long BankStatementId,
    IReadOnlyList<BankTransactionMatchRequest> Matches);

public sealed record BankTransactionMatchRequest(
    long BankTransactionId,
    long? PaymentId,
    long? ReceiptId);
