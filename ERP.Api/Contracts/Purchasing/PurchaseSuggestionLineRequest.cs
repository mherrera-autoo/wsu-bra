namespace ERP.Api.Contracts.Purchasing;

public sealed record PurchaseSuggestionLineRequest(
    long ProductId,
    decimal QtySuggested,
    long? SupplierSuggestedId,
    string Reason,
    long? WarehouseId);
