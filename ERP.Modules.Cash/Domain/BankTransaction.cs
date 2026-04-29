using ERP.Shared.Domain;
using ERP.Shared.Domain.ValueObjects;

namespace ERP.Modules.Cash.Domain;

public enum BankTransactionKind { Credit = 1, Debit = 2 }
public enum BankTransactionStatus { Unmatched = 1, Matched = 2 }

public sealed class BankTransaction : CompanyEntity
{
    public long BankStatementId { get; private set; }
    public DateTime TransactionDate { get; private set; } = DateTime.UtcNow;
    public Money Amount { get; private set; }
    public BankTransactionKind Kind { get; private set; }
    public BankTransactionStatus Status { get; private set; } = BankTransactionStatus.Unmatched;
    public string? Description { get; private set; }
    public string? Reference { get; private set; }
    public long? MatchedPaymentId { get; private set; }
    public long? MatchedReceiptId { get; private set; }

    private BankTransaction() { }

    public static BankTransaction Create(
        long companyId,
        long bankStatementId,
        Money amount,
        BankTransactionKind kind,
        DateTime? transactionDate = null,
        string? description = null,
        string? reference = null)
        => new()
        {
            CompanyId = companyId,
            BankStatementId = bankStatementId,
            Amount = amount,
            Kind = kind,
            TransactionDate = transactionDate ?? DateTime.UtcNow,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Reference = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim()
        };

    public void MatchPayment(long paymentId)
    {
        if (MatchedReceiptId.HasValue) throw new InvalidOperationException("Already matched to a receipt.");
        MatchedPaymentId = paymentId;
        Status = BankTransactionStatus.Matched;
    }

    public void MatchReceipt(long receiptId)
    {
        if (MatchedPaymentId.HasValue) throw new InvalidOperationException("Already matched to a payment.");
        MatchedReceiptId = receiptId;
        Status = BankTransactionStatus.Matched;
    }
}
