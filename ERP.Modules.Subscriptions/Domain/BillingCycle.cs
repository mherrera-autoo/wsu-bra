namespace ERP.Modules.Subscriptions.Domain;

public sealed class BillingCycle
{
    private BillingCycle() { }

    public long Id { get; private set; }
    public long SubscriptionId { get; private set; }
    public Subscription? Subscription { get; private set; }
    public BillingCycleType CycleType { get; private set; }
    public BillingCycleStatus Status { get; private set; }
    public DateTime StartsAt { get; private set; }
    public DateTime EndsAt { get; private set; }

    public static BillingCycle Create(Subscription subscription, BillingCycleType cycleType, DateTime startsAt, DateTime endsAt)
    {
        return new BillingCycle
        {
            Subscription = subscription,
            SubscriptionId = subscription.Id,
            CycleType = cycleType,
            Status = BillingCycleStatus.Open,
            StartsAt = startsAt,
            EndsAt = endsAt
        };
    }

    public void Close() => Status = BillingCycleStatus.Closed;
    public void MarkPastDue() => Status = BillingCycleStatus.PastDue;
}
