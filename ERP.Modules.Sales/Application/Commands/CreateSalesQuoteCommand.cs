namespace ERP.Modules.Sales.Application.Commands;

public sealed record CreateSalesQuoteCommand(
    string CustomerName,
    string? Notes,
    string? CurrencyCode,
    IReadOnlyList<CreateSalesQuoteLineCommand> Lines);

public sealed record CreateSalesQuoteLineCommand(
    long? ProductId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal? TaxRate);
