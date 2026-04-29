namespace ERP.Api.Contracts.Companies;

public sealed record CompanyUnitOfMeasureChangeRequest(
    string? DisplayNameOverride = null,
    int? SortOrder = null);
