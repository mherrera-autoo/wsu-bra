namespace ERP.Modules.Sales.Contracts;

public enum SalesQuoteStatus
{
    Draft = 1,
    Submitted = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 5
}

public sealed record CreateSalesQuoteApiRequest(
    string CustomerName,
    string? Notes,
    string? CurrencyCode,
    IReadOnlyList<CreateSalesQuoteLineRequest> Lines);

public sealed record CreateSalesQuoteLineRequest(
    long? ProductId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal? TaxRate);

public sealed record SalesQuoteLineDetail(
    int LineNumber,
    long? ProductId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    decimal? TaxRate);

public sealed record SalesQuoteDetail(
    Guid PublicId,
    string QuoteNumber,
    SalesQuoteStatus Status,
    string CustomerName,
    string? Notes,
    string? CurrencyCode,
    decimal Subtotal,
    decimal TaxTotal,
    decimal Total,
    IReadOnlyList<SalesQuoteLineDetail> Lines,
    DateTime CreatedAt,
    long CreatedByUserId,
    DateTime? UpdatedAt,
    long UpdatedByUserId);

public sealed record SalesQuoteListItem(
    Guid PublicId,
    string QuoteNumber,
    SalesQuoteStatus Status,
    string CustomerName,
    decimal Total,
    DateTime CreatedAt);

public sealed record SearchSalesQuotesQuery(
    SalesQuoteStatus? Status,
    string? Query,
    DateTime? From,
    DateTime? To,
    int Page,
    int PageSize);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

public sealed record RejectSalesQuoteRequest(string Reason);

public sealed record SalesQuoteCreated(long CompanyId, Guid QuotePublicId);

public sealed record SalesQuoteSubmitted(long CompanyId, Guid QuotePublicId);

public sealed record SalesQuoteApproved(long CompanyId, Guid QuotePublicId);

public sealed record SalesQuoteRejected(long CompanyId, Guid QuotePublicId, string Reason);
