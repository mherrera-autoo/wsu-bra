namespace ERP.Api.Contracts.Pricing;

public sealed record CreatePriceListRequest(
    long CompanyId,
    string Name,
    bool IsDefault);
