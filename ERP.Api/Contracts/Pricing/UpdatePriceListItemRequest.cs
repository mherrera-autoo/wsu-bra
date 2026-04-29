namespace ERP.Api.Contracts.Pricing;

public sealed record UpdatePriceListItemRequest(
    long CompanyId,
    long PriceListId,
    long ProductId,
    decimal UnitPriceAmount,
    string UnitPriceCurrency);
