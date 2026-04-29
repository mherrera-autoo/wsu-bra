namespace ERP.Api.Contracts.Purchasing;

public sealed record PurchaseSuggestionLineResponse(
    long ProductId,
    decimal QtySuggested,
    long? SupplierSuggestedId,
    string Reason,
    long? WarehouseId);

public sealed record PurchaseSuggestionResponse(
    long Id,
    string Status,
    long? SupplierSuggestedId,
    long? ConvertedPurchaseOrderId,
    DateTime CreatedAt,
    IReadOnlyList<PurchaseSuggestionLineResponse> Lines);
