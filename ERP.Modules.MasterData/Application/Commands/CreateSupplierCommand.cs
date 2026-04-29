namespace ERP.Modules.MasterData.Application.Commands;

public sealed record CreateSupplierCommand(
    long CompanyId,
    string Name,
    string? TaxId,
    string? Country,
    string? Currency);
