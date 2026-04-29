using ERP.Modules.Purchasing.Domain;

namespace ERP.Modules.Purchasing.Application.Services;

public sealed record PurchaseSuggestionMinimumInput(long ProductId, decimal MinimumQty, long? WarehouseId);

public sealed record PurchaseSuggestionCreateInput(long? SupplierSuggestedId, IReadOnlyList<PurchaseSuggestionLineInput> Lines);

public sealed record PurchaseSuggestionUpdateInput(IReadOnlyList<PurchaseSuggestionLineInput> Lines);

public sealed record PurchaseSuggestionGenerateInput(
    long? WarehouseId,
    long? SupplierId,
    IReadOnlyList<PurchaseSuggestionMinimumInput> Minimums);

public sealed record PurchaseSuggestionConvertInput(long? SupplierId, string? Currency);
