namespace ERP.Modules.Subscriptions.Domain;

public sealed class SubscriptionUsage
{
    private SubscriptionUsage() { }

    public long Id { get; private set; }
    public long SubscriptionId { get; private set; }
    public Subscription? Subscription { get; private set; }
    public string Metric { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public DateTime RecordedAt { get; private set; }

    public static SubscriptionUsage Create(Subscription subscription, string metric, decimal quantity, DateTime recordedAt)
    {
        return new SubscriptionUsage
        {
            Subscription = subscription,
            SubscriptionId = subscription.Id,
            Metric = metric.Trim(),
            Quantity = quantity,
            RecordedAt = recordedAt
        };
    }
}
