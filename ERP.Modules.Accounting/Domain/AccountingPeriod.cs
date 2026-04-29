namespace ERP.Modules.Accounting.Domain;

public sealed class AccountingPeriod
{
    public long Id { get; private set; }
    public long CompanyId { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public AccountingPeriodStatus Status { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public long? ClosedBy { get; private set; }

    private AccountingPeriod() { }

    public static AccountingPeriod Open(long companyId, int year, int month)
        => new()
        {
            CompanyId = companyId,
            Year = year,
            Month = month,
            Status = AccountingPeriodStatus.Open
        };

    public void Close(DateTime closedAt, long closedBy)
    {
        Status = AccountingPeriodStatus.Closed;
        ClosedAt = closedAt;
        ClosedBy = closedBy;
    }
}

public enum AccountingPeriodStatus
{
    Open,
    Closed
}
