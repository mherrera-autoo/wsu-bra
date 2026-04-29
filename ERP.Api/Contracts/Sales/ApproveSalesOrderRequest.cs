namespace ERP.Api.Contracts.Sales;

public sealed record ApproveSalesOrderRequest(long CompanyId, long? WarehouseId);
