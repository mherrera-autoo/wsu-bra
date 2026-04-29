namespace ERP.Api.Contracts.Purchasing;

public sealed record ConvertPurchaseSuggestionRequest(long? SupplierId, string? Currency);
