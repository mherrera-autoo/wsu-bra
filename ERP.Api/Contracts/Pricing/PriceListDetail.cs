namespace ERP.Api.Contracts.Pricing;

public sealed record PriceListDetail(
    long Id,
    string Name,
    bool IsDefault,
    IReadOnlyList<PriceListItemSummary> Items);
