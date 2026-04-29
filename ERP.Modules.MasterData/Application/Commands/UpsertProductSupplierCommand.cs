namespace ERP.Modules.MasterData.Application.Commands;

public sealed record UpsertProductSupplierCommand(
    long CompanyId,
    long ProductId,
    long SupplierId,
    string? SupplierSku,
    decimal UnitPurchasePrice,
    decimal MinimumPurchaseLot);
