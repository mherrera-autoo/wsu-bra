namespace ERP.Api.Contracts.Purchasing;

public sealed record CreatePurchaseSuggestionRequest(
    long CompanyId,
    long? SupplierSuggestedId,
    IReadOnlyList<PurchaseSuggestionLineRequest> Lines);
