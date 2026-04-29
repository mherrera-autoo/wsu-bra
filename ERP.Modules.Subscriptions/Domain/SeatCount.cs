namespace ERP.Modules.Subscriptions.Domain;

public sealed class SeatCount
{
    private SeatCount() { }

    public long Id { get; private set; }
    public long SubscriptionId { get; private set; }
    public Subscription? Subscription { get; private set; }
    public int MaxSeats { get; private set; }
    public int UsedSeats { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static SeatCount Create(Subscription subscription, int maxSeats, int usedSeats)
    {
        return new SeatCount
        {
            Subscription = subscription,
            SubscriptionId = subscription.Id,
            MaxSeats = maxSeats,
            UsedSeats = usedSeats,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateUsage(int usedSeats)
    {
        UsedSeats = usedSeats;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLimit(int maxSeats)
    {
        MaxSeats = maxSeats;
        UpdatedAt = DateTime.UtcNow;
    }
}
