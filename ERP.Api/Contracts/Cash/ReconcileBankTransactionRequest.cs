namespace ERP.Api.Contracts.Cash;

public sealed record ReconcileBankTransactionRequest(
    long CompanyId,
    long? ReceiptId,
    long? PaymentId);
