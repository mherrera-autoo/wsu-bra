using ERP.Modules.Accounting.Application.Repositories;
using ERP.Modules.Accounting.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.Accounting.Application.Services;

public sealed record ReceivableScheduleSnapshot(DateTime DueDate, decimal Amount, ReceivableScheduleStatus Status);
public sealed record ReceivableSnapshot(
    long Id,
    long CustomerId,
    ReceivableOrigin Origin,
    ReceivableStatus Status,
    string Currency,
    decimal TotalAmount,
    decimal OutstandingAmount,
    DateTime IssueDate,
    IReadOnlyCollection<ReceivableScheduleSnapshot> Schedules);

public sealed record ReceivableAgingBucket(string Bucket, decimal Amount);
public sealed record ReceivableAgingSnapshot(long CompanyId, DateTime AsOf, IReadOnlyCollection<ReceivableAgingBucket> Buckets, decimal TotalOutstanding);

public sealed class ReceivablesService
{
    private static readonly string[] AgingBuckets = ["Current", "1-30", "31-60", "61-90", "90+"];

    private readonly IAccountsReceivableRepository _accountsReceivableRepository;

    public ReceivablesService(IAccountsReceivableRepository accountsReceivableRepository)
    {
        _accountsReceivableRepository = accountsReceivableRepository;
    }

    public async Task<Result<AccountsReceivable>> CreateFromInvoiceAsync(
        long companyId,
        long customerId,
        long sourceDocumentId,
        DateTime issueDate,
        DateTime dueDate,
        string currency,
        decimal totalAmount,
        CancellationToken cancellationToken = default)
    {
        var receivable = AccountsReceivable.CreateInvoice(
            companyId,
            customerId,
            sourceDocumentId,
            issueDate,
            dueDate,
            currency,
            totalAmount);

        await _accountsReceivableRepository.AddAsync(receivable, cancellationToken);
        return Result<AccountsReceivable>.Ok(receivable);
    }

    public async Task<Result<AccountsReceivable>> CreateFromCreditNoteAsync(
        long companyId,
        long customerId,
        long sourceDocumentId,
        DateTime issueDate,
        string currency,
        decimal totalAmount,
        CancellationToken cancellationToken = default)
    {
        var receivable = AccountsReceivable.CreateCreditNote(
            companyId,
            customerId,
            sourceDocumentId,
            issueDate,
            currency,
            totalAmount);

        await _accountsReceivableRepository.AddAsync(receivable, cancellationToken);
        return Result<AccountsReceivable>.Ok(receivable);
    }

    public async Task<IReadOnlyList<ReceivableSnapshot>> GetReceivablesAsync(
        long companyId,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        var receivables = await _accountsReceivableRepository.GetByCompanyAsync(companyId, cancellationToken);
        return receivables
            .Select(receivable => MapSnapshot(receivable, asOf))
            .ToList();
    }

    public async Task<ReceivableAgingSnapshot> GetAgingAsync(
        long companyId,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        var receivables = await _accountsReceivableRepository.GetByCompanyAsync(companyId, cancellationToken);
        var buckets = AgingBuckets.ToDictionary(bucket => bucket, _ => 0m);
        var asOfDate = asOf.Date;

        foreach (var receivable in receivables)
        {
            foreach (var schedule in receivable.Schedules)
            {
                if (schedule.Status is ReceivableScheduleStatus.Paid)
                {
                    continue;
                }

                var dueDate = schedule.DueDate.Date;
                if (dueDate >= asOfDate)
                {
                    buckets["Current"] += schedule.Amount;
                    continue;
                }

                var daysPastDue = (asOfDate - dueDate).Days;
                if (daysPastDue <= 30)
                {
                    buckets["1-30"] += schedule.Amount;
                }
                else if (daysPastDue <= 60)
                {
                    buckets["31-60"] += schedule.Amount;
                }
                else if (daysPastDue <= 90)
                {
                    buckets["61-90"] += schedule.Amount;
                }
                else
                {
                    buckets["90+"] += schedule.Amount;
                }
            }
        }

        var bucketSnapshots = AgingBuckets
            .Select(bucket => new ReceivableAgingBucket(bucket, buckets[bucket]))
            .ToList();

        var totalOutstanding = bucketSnapshots.Sum(bucket => bucket.Amount);

        return new ReceivableAgingSnapshot(companyId, asOfDate, bucketSnapshots, totalOutstanding);
    }

    private static ReceivableSnapshot MapSnapshot(AccountsReceivable receivable, DateTime asOf)
    {
        var schedules = receivable.Schedules
            .Select(schedule => MapScheduleSnapshot(schedule, asOf))
            .ToList();

        var outstanding = schedules
            .Where(schedule => schedule.Status is ReceivableScheduleStatus.Pending or ReceivableScheduleStatus.Overdue)
            .Sum(schedule => schedule.Amount);

        var status = receivable.Status == ReceivableStatus.Cancelled
            ? ReceivableStatus.Cancelled
            : outstanding == 0
                ? ReceivableStatus.Closed
                : ReceivableStatus.Open;

        return new ReceivableSnapshot(
            receivable.Id,
            receivable.CustomerId,
            receivable.Origin,
            status,
            receivable.Currency,
            receivable.TotalAmount,
            outstanding,
            receivable.IssueDate,
            schedules);
    }

    private static ReceivableScheduleSnapshot MapScheduleSnapshot(ReceivableSchedule schedule, DateTime asOf)
    {
        var status = schedule.Status;
        if (status == ReceivableScheduleStatus.Pending && schedule.DueDate.Date < asOf.Date)
        {
            status = ReceivableScheduleStatus.Overdue;
        }

        return new ReceivableScheduleSnapshot(schedule.DueDate, schedule.Amount, status);
    }
}
