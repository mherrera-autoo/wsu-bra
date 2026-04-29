namespace ERP.Api.Contracts.MasterData;

public sealed record UpsertProductSupplierRequest(
    string? SupplierSku,
    decimal UnitPurchasePrice,
    decimal MinimumPurchaseLot);
