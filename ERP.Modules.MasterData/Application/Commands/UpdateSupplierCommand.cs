namespace ERP.Modules.MasterData.Application.Commands;

public sealed record UpdateSupplierCommand(
    long CompanyId,
    long SupplierId,
    string Name,
    string? TaxId,
    string? Country,
    string? Currency,
    bool IsActive);
