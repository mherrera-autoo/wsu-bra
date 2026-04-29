namespace ERP.Modules.Subscriptions.Domain;

public sealed class Plan
{
    private readonly List<PlanFeature> _features = new();

    private Plan() { }

    public long Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal PriceAmount { get; private set; }
    public string PriceCurrency { get; private set; } = "CLP";
    public BillingCycleType BillingCycle { get; private set; }
    public int? SeatLimit { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<PlanFeature> Features => _features.AsReadOnly();

    public static Plan Create(
        string code,
        string name,
        decimal priceAmount,
        string priceCurrency,
        BillingCycleType billingCycle,
        int? seatLimit,
        IEnumerable<string> features,
        string? description = null)
    {
        var plan = new Plan
        {
            Code = code.Trim(),
            Name = name.Trim(),
            Description = description?.Trim(),
            PriceAmount = priceAmount,
            PriceCurrency = priceCurrency.Trim(),
            BillingCycle = billingCycle,
            SeatLimit = seatLimit,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var feature in features.Where(f => !string.IsNullOrWhiteSpace(f)))
        {
            plan._features.Add(PlanFeature.Create(feature.Trim()));
        }

        return plan;
    }

    public void UpdatePricing(decimal priceAmount, string priceCurrency, BillingCycleType billingCycle, int? seatLimit)
    {
        PriceAmount = priceAmount;
        PriceCurrency = priceCurrency.Trim();
        BillingCycle = billingCycle;
        SeatLimit = seatLimit;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void ReplaceFeatures(IEnumerable<string> features)
    {
        _features.Clear();
        foreach (var feature in features.Where(f => !string.IsNullOrWhiteSpace(f)))
        {
            _features.Add(PlanFeature.Create(feature.Trim()));
        }
    }
}
