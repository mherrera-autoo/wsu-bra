namespace ERP.Api.Contracts.MasterData;

public sealed record SupplierSummary(
    long Id,
    string Name,
    string? TaxId,
    string? Country,
    string? Currency,
    bool IsActive);
