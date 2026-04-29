namespace ERP.Modules.MasterData.Application.Commands;

public sealed record DeleteProductSupplierCommand(
    long CompanyId,
    long ProductId,
    long SupplierId);
