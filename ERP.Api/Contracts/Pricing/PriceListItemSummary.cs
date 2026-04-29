namespace ERP.Api.Contracts.Pricing;

public sealed record PriceListItemSummary(
    long Id,
    long ProductId,
    decimal UnitPriceAmount,
    string UnitPriceCurrency);
