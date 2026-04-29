namespace ERP.Api.Contracts.Tax;

public sealed record CreateTaxRequest(
    long CompanyId,
    string Code,
    string Name,
    decimal Rate);
