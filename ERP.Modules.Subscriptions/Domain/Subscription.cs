namespace ERP.Modules.Subscriptions.Domain;

public sealed class Subscription
{
    private Subscription() { }

    public long Id { get; private set; }
    public long CompanyId { get; private set; }
    public long PlanId { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public BillingCycle? CurrentBillingCycle { get; private set; }
    public Trial? Trial { get; private set; }
    public SeatCount? SeatCount { get; private set; }
    public Plan? Plan { get; private set; }

    public static Subscription Create(long companyId, long planId, DateTime startedAt, SubscriptionStatus status)
    {
        return new Subscription
        {
            CompanyId = companyId,
            PlanId = planId,
            StartedAt = startedAt,
            Status = status
        };
    }

    public void Activate()
    {
        Status = SubscriptionStatus.Active;
        CancelledAt = null;
    }

    public void Cancel(DateTime cancelledAt)
    {
        Status = SubscriptionStatus.Cancelled;
        CancelledAt = cancelledAt;
    }

    public void UpdatePlan(long planId)
    {
        PlanId = planId;
    }

    public void AttachTrial(Trial trial)
    {
        Trial = trial;
    }

    public void AttachBillingCycle(BillingCycle billingCycle)
    {
        CurrentBillingCycle = billingCycle;
    }

    public void AttachSeatCount(SeatCount seatCount)
    {
        SeatCount = seatCount;
    }

    public void MarkPastDue() => Status = SubscriptionStatus.PastDue;

    public void Suspend() => Status = SubscriptionStatus.Suspended;
}
