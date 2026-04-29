namespace ERP.Api.Contracts.Companies;

public sealed record CreateCompanyCurrencyRequest(
    long CurrencyId,
    bool IsDefault,
    bool IsActive = true);
