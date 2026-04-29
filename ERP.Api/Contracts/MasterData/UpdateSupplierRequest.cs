namespace ERP.Api.Contracts.MasterData;

public sealed record UpdateSupplierRequest(
    long CompanyId,
    string Name,
    string? TaxId,
    string? Country,
    string? Currency,
    bool IsActive);
