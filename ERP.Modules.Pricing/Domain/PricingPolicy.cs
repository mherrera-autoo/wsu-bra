using ERP.Shared.Domain;

namespace ERP.Modules.Pricing.Domain;

public enum PricingRoundingMode
{
    None = 0,
    Nearest = 1,
    Up = 2,
    Down = 3
}

public sealed class PricingPolicy : CompanyEntity
{
    public decimal MarginPercent { get; private set; }
    public PricingRoundingMode RoundingMode { get; private set; }
    public int? DecimalPlaces { get; private set; }
    public long UpdatedByUserId { get; private set; }

    private PricingPolicy() { }

    public static PricingPolicy Create(
        long companyId,
        decimal marginPercent,
        PricingRoundingMode roundingMode,
        int? decimalPlaces,
        long updatedByUserId)
    {
        Validate(marginPercent, roundingMode, decimalPlaces);

        return new PricingPolicy
        {
            CompanyId = companyId,
            MarginPercent = marginPercent,
            RoundingMode = roundingMode,
            DecimalPlaces = decimalPlaces,
            UpdatedByUserId = updatedByUserId
        };
    }

    public void Update(decimal marginPercent, PricingRoundingMode roundingMode, int? decimalPlaces, long updatedByUserId)
    {
        Validate(marginPercent, roundingMode, decimalPlaces);

        MarginPercent = marginPercent;
        RoundingMode = roundingMode;
        DecimalPlaces = decimalPlaces;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void Validate(decimal marginPercent, PricingRoundingMode roundingMode, int? decimalPlaces)
    {
        if (marginPercent < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(marginPercent), "Margin percent must be >= 0.");
        }

        if (roundingMode != PricingRoundingMode.None && decimalPlaces is null)
        {
            throw new ArgumentException("Decimal places must be defined when rounding is enabled.", nameof(decimalPlaces));
        }

        if (decimalPlaces is not null && decimalPlaces < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(decimalPlaces), "Decimal places must be >= 0.");
        }
    }
}
