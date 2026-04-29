using ERP.Shared.Domain;
using ERP.Shared.Domain.ValueObjects;

namespace ERP.Modules.Cash.Domain;

public enum ReceiptStatus
{
    Draft = 1,
    Posted = 2,
    Applied = 3,
    Cancelled = 4
}

public sealed class Receipt : CompanyEntity
{
    public long CustomerId { get; private set; }
    public long? SalesDocumentId { get; private set; }
    public long? BankAccountId { get; private set; }
    public Money Amount { get; private set; }
    public decimal AppliedAmount { get; private set; }
    public DateTime ReceivedAt { get; private set; }
    public string? Reference { get; private set; }
    public ReceiptStatus Status { get; private set; } = ReceiptStatus.Draft;

    private Receipt() { }

    public static Receipt Create(
        long companyId,
        long customerId,
        long bankAccountId,
        Money amount,
        DateTime receivedAt,
        string? reference)
    {
        if (amount.Amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        return new Receipt
        {
            CompanyId = companyId,
            CustomerId = customerId,
            BankAccountId = bankAccountId,
            Amount = amount,
            ReceivedAt = receivedAt,
            Reference = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim()
        };
    }

    public static Receipt Create(
        long companyId,
        long customerId,
        long? salesDocumentId,
        Money amount,
        DateTime? receivedAt = null,
        string? reference = null)
    {
        if (amount.Amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        return new Receipt
        {
            CompanyId = companyId,
            CustomerId = customerId,
            SalesDocumentId = salesDocumentId,
            Amount = amount,
            ReceivedAt = receivedAt ?? DateTime.UtcNow,
            Reference = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim()
        };
    }

    public void Post()
    {
        if (Status != ReceiptStatus.Draft)
        {
            throw new InvalidOperationException("Only draft receipts can be posted.");
        }

        Status = ReceiptStatus.Posted;
    }

    public void Apply(decimal amount)
    {
        if (Status == ReceiptStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot apply a cancelled receipt.");
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        if (AppliedAmount + amount > Amount.Amount)
        {
            throw new InvalidOperationException("Applied amount exceeds receipt amount.");
        }

        AppliedAmount += amount;
        if (AppliedAmount == Amount.Amount)
        {
            Status = ReceiptStatus.Applied;
        }
    }

    public void Cancel()
    {
        if (AppliedAmount > 0)
        {
            throw new InvalidOperationException("Cannot cancel an applied receipt.");
        }

        Status = ReceiptStatus.Cancelled;
    }
}
