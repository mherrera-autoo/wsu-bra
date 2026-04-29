namespace ERP.Api.Contracts.Pharmacy;

// TODO: Move sales assist DTOs to ERP.Api.Contracts.RetailPharmacy (ERP.Modules.RetailPharmacy).
public sealed record SalesRecommendationQuery(
    long CustomerId,
    int? Limit);
