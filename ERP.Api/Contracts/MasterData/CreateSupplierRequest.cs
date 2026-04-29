namespace ERP.Api.Contracts.MasterData;

public sealed record CreateSupplierRequest(
    long CompanyId,
    string Name,
    string? TaxId,
    string? Country,
    string? Currency);
