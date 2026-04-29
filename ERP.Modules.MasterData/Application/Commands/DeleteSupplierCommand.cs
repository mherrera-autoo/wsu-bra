namespace ERP.Modules.MasterData.Application.Commands;

public sealed record DeleteSupplierCommand(long CompanyId, long SupplierId);
