using ERP.Shared.Domain;

namespace ERP.Modules.Accounting.Domain;

public enum ReceivableOrigin { Invoice = 1, CreditNote = 2 }
public enum ReceivableStatus { Open = 1, Closed = 2, Cancelled = 3 }
public enum ReceivableScheduleStatus { Pending = 1, Paid = 2, Overdue = 3 }

public sealed class AccountsReceivable : CompanyEntity
{
    public long CustomerId { get; private set; }
    public ReceivableOrigin Origin { get; private set; }
    public long SourceDocumentId { get; private set; }
    public DateTime IssueDate { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public decimal OutstandingAmount { get; private set; }
    public ReceivableStatus Status { get; private set; } = ReceivableStatus.Open;
    public List<ReceivableSchedule> Schedules { get; private set; } = new();

    private AccountsReceivable() { }

    public static AccountsReceivable CreateInvoice(
        long companyId,
        long customerId,
        long sourceDocumentId,
        DateTime issueDate,
        DateTime dueDate,
        string currency,
        decimal totalAmount)
    {
        if (totalAmount <= 0) throw new ArgumentOutOfRangeException(nameof(totalAmount));
        var receivable = new AccountsReceivable
        {
            CompanyId = companyId,
            CustomerId = customerId,
            SourceDocumentId = sourceDocumentId,
            IssueDate = issueDate,
            Currency = currency,
            Origin = ReceivableOrigin.Invoice,
            TotalAmount = totalAmount,
            OutstandingAmount = totalAmount,
            Status = ReceivableStatus.Open
        };
        receivable.AddSchedule(dueDate, totalAmount, ReceivableScheduleStatus.Pending);
        return receivable;
    }

    public static AccountsReceivable CreateCreditNote(
        long companyId,
        long customerId,
        long sourceDocumentId,
        DateTime issueDate,
        string currency,
        decimal totalAmount)
    {
        if (totalAmount <= 0) throw new ArgumentOutOfRangeException(nameof(totalAmount));
        var receivable = new AccountsReceivable
        {
            CompanyId = companyId,
            CustomerId = customerId,
            SourceDocumentId = sourceDocumentId,
            IssueDate = issueDate,
            Currency = currency,
            Origin = ReceivableOrigin.CreditNote,
            TotalAmount = -Math.Abs(totalAmount),
            OutstandingAmount = 0,
            Status = ReceivableStatus.Closed
        };
        receivable.AddSchedule(issueDate, -Math.Abs(totalAmount), ReceivableScheduleStatus.Paid);
        return receivable;
    }

    private void AddSchedule(DateTime dueDate, decimal amount, ReceivableScheduleStatus status)
    {
        Schedules.Add(ReceivableSchedule.Create(CompanyId, Id, dueDate, amount, status));
    }
}

public sealed class ReceivableSchedule : CompanyEntity
{
    public long AccountsReceivableId { get; private set; }
    public DateTime DueDate { get; private set; }
    public decimal Amount { get; private set; }
    public ReceivableScheduleStatus Status { get; private set; }

    private ReceivableSchedule() { }

    public static ReceivableSchedule Create(
        long companyId,
        long accountsReceivableId,
        DateTime dueDate,
        decimal amount,
        ReceivableScheduleStatus status)
    {
        return new ReceivableSchedule
        {
            CompanyId = companyId,
            AccountsReceivableId = accountsReceivableId,
            DueDate = dueDate,
            Amount = amount,
            Status = status
        };
    }
}
