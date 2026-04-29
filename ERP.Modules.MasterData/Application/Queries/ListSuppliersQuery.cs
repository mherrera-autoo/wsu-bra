namespace ERP.Modules.MasterData.Application.Queries;

public sealed record ListSuppliersQuery(
    long CompanyId,
    string? Search,
    bool? IsActive);
