namespace ERP.Api.Contracts.Purchasing;

public sealed record PurchaseSuggestionMinimumRequest(
    long ProductId,
    decimal MinimumQty,
    long? WarehouseId);

public sealed record GeneratePurchaseSuggestionsRequest(
    long? WarehouseId,
    long? SupplierId,
    IReadOnlyList<PurchaseSuggestionMinimumRequest>? Minimums);
