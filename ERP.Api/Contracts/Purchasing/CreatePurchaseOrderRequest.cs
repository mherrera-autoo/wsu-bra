using ERP.Api.Contracts.Tax;

namespace ERP.Api.Contracts.Purchasing;

public sealed record CreatePurchaseOrderRequest(
    long CompanyId,
    long SupplierId,
    string Currency,
    IReadOnlyList<CreatePurchaseOrderLineRequest> Lines);

public sealed record CreatePurchaseOrderLineRequest(
    long ProductId,
    decimal Qty,
    decimal UnitPriceAmount,
    string UnitPriceCurrency,
    long? TaxGroupId,
    TaxGroupRequest? TaxGroup);
