namespace ERP.Api.Contracts.Sales;

public sealed record SalesDocumentSummary(
    long Id,
    long CustomerId,
    string Kind,
    string Status,
    DateTime CreatedAt);
