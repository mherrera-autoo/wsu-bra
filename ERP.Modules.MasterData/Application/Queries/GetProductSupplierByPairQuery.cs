namespace ERP.Modules.MasterData.Application.Queries;

public sealed record GetProductSupplierByPairQuery(long CompanyId, long ProductId, long SupplierId);
