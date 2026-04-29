namespace ERP.Api.Contracts.Companies;

public sealed record UpdateCompanyCurrencyRequest(
    long CompanyId,
    bool IsDefault,
    bool IsActive);
