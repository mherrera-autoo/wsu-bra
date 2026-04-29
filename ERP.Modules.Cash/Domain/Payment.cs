using ERP.Shared.Domain;
using ERP.Shared.Domain.ValueObjects;

namespace ERP.Modules.Cash.Domain;

public enum PaymentStatus
{
    Draft = 1,
    Posted = 2,
    Applied = 3,
    Cancelled = 4
}

public sealed class Payment : CompanyEntity
{
    public long SupplierId { get; private set; }
    public long? PurchaseOrderId { get; private set; }
    public long? BankAccountId { get; private set; }
    public Money Amount { get; private set; }
    public decimal AppliedAmount { get; private set; }
    public DateTime PaidAt { get; private set; }
    public string? Reference { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Draft;

    private Payment() { }

    public static Payment Create(
        long companyId,
        long supplierId,
        long bankAccountId,
        Money amount,
        DateTime paidAt,
        string? reference)
    {
        if (amount.Amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        return new Payment
        {
            CompanyId = companyId,
            SupplierId = supplierId,
            BankAccountId = bankAccountId,
            Amount = amount,
            PaidAt = paidAt,
            Reference = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim()
        };
    }

    public static Payment Create(
        long companyId,
        long supplierId,
        long? purchaseOrderId,
        Money amount,
        DateTime? paidAt = null,
        string? reference = null)
    {
        if (amount.Amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        return new Payment
        {
            CompanyId = companyId,
            SupplierId = supplierId,
            PurchaseOrderId = purchaseOrderId,
            Amount = amount,
            PaidAt = paidAt ?? DateTime.UtcNow,
            Reference = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim()
        };
    }

    public void Post()
    {
        if (Status != PaymentStatus.Draft)
        {
            throw new InvalidOperationException("Only draft payments can be posted.");
        }

        Status = PaymentStatus.Posted;
    }

    public void Apply(decimal amount)
    {
        if (Status == PaymentStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot apply a cancelled payment.");
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        if (AppliedAmount + amount > Amount.Amount)
        {
            throw new InvalidOperationException("Applied amount exceeds payment amount.");
        }

        AppliedAmount += amount;
        if (AppliedAmount == Amount.Amount)
        {
            Status = PaymentStatus.Applied;
        }
    }

    public void Cancel()
    {
        if (AppliedAmount > 0)
        {
            throw new InvalidOperationException("Cannot cancel an applied payment.");
        }

        Status = PaymentStatus.Cancelled;
    }
}
