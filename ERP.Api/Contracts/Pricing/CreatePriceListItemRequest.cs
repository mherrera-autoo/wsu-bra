namespace ERP.Api.Contracts.Pricing;

public sealed record CreatePriceListItemRequest(
    long CompanyId,
    long PriceListId,
    long ProductId,
    decimal UnitPriceAmount,
    string UnitPriceCurrency);
