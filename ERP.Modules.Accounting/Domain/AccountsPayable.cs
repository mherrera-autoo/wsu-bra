using ERP.Shared.Domain;
using ERP.Shared.Domain.ValueObjects;

namespace ERP.Modules.Accounting.Domain;

public enum PayableSourceType
{
    PurchaseOrder = 1,
    GoodsReceipt = 2
}

public enum AccountsPayableStatus
{
    Open = 1,
    PartiallyPaid = 2,
    Paid = 3,
    Cancelled = 4
}

public enum PayableScheduleStatus
{
    Pending = 1,
    Paid = 2,
    Overdue = 3
}

public sealed class AccountsPayable : CompanyEntity
{
    public long SupplierId { get; private set; }
    public PayableSourceType SourceType { get; private set; }
    public long SourceId { get; private set; }
    public AccountsPayableStatus Status { get; private set; } = AccountsPayableStatus.Open;
    public Money TotalAmount { get; private set; }
    public List<PayableSchedule> Schedules { get; private set; } = new();

    private AccountsPayable() { }

    public static AccountsPayable Create(long companyId, long supplierId, PayableSourceType sourceType, long sourceId, Money totalAmount)
    {
        if (totalAmount.Amount <= 0) throw new ArgumentOutOfRangeException(nameof(totalAmount));
        return new()
        {
            CompanyId = companyId,
            SupplierId = supplierId,
            SourceType = sourceType,
            SourceId = sourceId,
            TotalAmount = totalAmount
        };
    }

    public void AddSchedule(DateTime dueDate, Money amount, DateTime? asOf = null)
    {
        if (amount.Amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (!string.Equals(amount.Currency, TotalAmount.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Schedule currency must match payable currency.");
        }

        Schedules.Add(PayableSchedule.Create(CompanyId, Id, dueDate, amount, asOf ?? DateTime.UtcNow));
    }

    public void RefreshStatus(DateTime? asOf = null)
    {
        var now = asOf ?? DateTime.UtcNow;
        foreach (var schedule in Schedules)
        {
            schedule.RefreshStatus(now);
        }

        if (Schedules.All(s => s.Status == PayableScheduleStatus.Paid))
        {
            Status = AccountsPayableStatus.Paid;
        }
        else if (Schedules.Any(s => s.Status == PayableScheduleStatus.Paid))
        {
            Status = AccountsPayableStatus.PartiallyPaid;
        }
        else
        {
            Status = AccountsPayableStatus.Open;
        }
    }
}

public sealed class PayableSchedule : CompanyEntity
{
    public long AccountsPayableId { get; private set; }
    public DateTime DueDate { get; private set; }
    public PayableScheduleStatus Status { get; private set; } = PayableScheduleStatus.Pending;
    public Money Amount { get; private set; }

    private PayableSchedule() { }

    public static PayableSchedule Create(long companyId, long payableId, DateTime dueDate, Money amount, DateTime asOf)
    {
        var schedule = new PayableSchedule
        {
            CompanyId = companyId,
            AccountsPayableId = payableId,
            DueDate = dueDate,
            Amount = amount
        };

        schedule.RefreshStatus(asOf);
        return schedule;
    }

    public void MarkPaid()
    {
        Status = PayableScheduleStatus.Paid;
    }

    public void RefreshStatus(DateTime asOf)
    {
        if (Status == PayableScheduleStatus.Paid)
        {
            return;
        }

        Status = DueDate.Date < asOf.Date ? PayableScheduleStatus.Overdue : PayableScheduleStatus.Pending;
    }
}
