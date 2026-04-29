namespace ERP.Api.Contracts.Purchasing;

public sealed record UpdatePurchaseSuggestionRequest(
    IReadOnlyList<PurchaseSuggestionLineRequest> Lines);
