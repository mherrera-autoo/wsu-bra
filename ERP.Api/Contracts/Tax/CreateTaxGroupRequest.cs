namespace ERP.Api.Contracts.Tax;

public sealed record CreateTaxGroupRequest(
    long CompanyId,
    string Name);
