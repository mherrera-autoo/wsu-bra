namespace ERP.Modules.Subscriptions.Domain;

public sealed class Trial
{
    private Trial() { }

    public long Id { get; private set; }
    public long SubscriptionId { get; private set; }
    public Subscription? Subscription { get; private set; }
    public DateTime StartsAt { get; private set; }
    public DateTime EndsAt { get; private set; }
    public TrialStatus Status { get; private set; }

    public static Trial Create(Subscription subscription, DateTime startsAt, DateTime endsAt)
    {
        return new Trial
        {
            Subscription = subscription,
            SubscriptionId = subscription.Id,
            StartsAt = startsAt,
            EndsAt = endsAt,
            Status = TrialStatus.Active
        };
    }

    public void Cancel() => Status = TrialStatus.Cancelled;

    public void Expire() => Status = TrialStatus.Expired;
}
