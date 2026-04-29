namespace ERP.Api.Contracts.Sales;

public sealed record SalesDocumentLineDetail(long ProductId, decimal Qty, decimal UnitPriceAmount, string UnitPriceCurrency);

public sealed record SalesDocumentDetail(
    long Id,
    long CustomerId,
    string Kind,
    string Status,
    DateTime CreatedAt,
    IReadOnlyList<SalesDocumentLineDetail> Lines);
