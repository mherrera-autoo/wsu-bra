using ERP.Modules.Pricing.Domain;

namespace ERP.Api.Contracts.Pricing;

public sealed record UpsertPricingPolicyRequest(
    long CompanyId,
    decimal MarginPercent,
    PricingRoundingMode RoundingMode,
    int? DecimalPlaces);
