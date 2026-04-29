namespace ERP.Api.Contracts.Companies;

public sealed record CompanyCurrencySummary(
    long CurrencyId,
    string Code,
    string Name,
    int Order,
    string? Symbol,
    bool IsAssigned,
    bool IsDefault,
    bool IsActive);
