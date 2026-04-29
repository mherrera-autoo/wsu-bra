namespace ERP.Modules.Sales.Application.Commands;

public sealed record SubmitSalesQuoteCommand(Guid QuotePublicId);

public sealed record ApproveSalesQuoteCommand(Guid QuotePublicId);

public sealed record RejectSalesQuoteCommand(Guid QuotePublicId, string Reason);
