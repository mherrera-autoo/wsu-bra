namespace ERP.Api.Contracts.Pricing;

public sealed record PriceListSummary(
    long Id,
    string Name,
    bool IsDefault);
