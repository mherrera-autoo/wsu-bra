namespace ERP.Api.Contracts.Tax;

public sealed record AddTaxRuleRequest(
    long CompanyId,
    long TaxId,
    decimal Rate,
    int Sequence = 1);
