namespace ERP.Modules.Subscriptions.Domain;

public sealed class PlanFeature
{
    private PlanFeature() { }

    public long Id { get; private set; }
    public long PlanId { get; private set; }
    public string FeatureKey { get; private set; } = string.Empty;

    public static PlanFeature Create(string featureKey)
    {
        return new PlanFeature
        {
            FeatureKey = featureKey.Trim()
        };
    }
}
