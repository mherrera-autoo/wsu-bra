using ERP.Api.Contracts.Tax;

namespace ERP.Api.Contracts.Sales;

public sealed record CreateSalesQuoteRequest(
    long CompanyId,
    long CustomerId,
    IReadOnlyList<CreateSalesLineRequest> Lines);

public sealed record CreateSalesLineRequest(
    long ProductId,
    decimal Qty,
    decimal UnitPriceAmount,
    string UnitPriceCurrency,
    long? TaxGroupId,
    TaxGroupRequest? TaxGroup);
