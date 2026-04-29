namespace ERP.Api.Contracts.Tax;

public sealed record TaxRuleRequest(
    string Code,
    string Name,
    decimal Rate,
    int Sequence = 1,
    bool IsCompound = false);

public sealed record TaxGroupRequest(
    string Name,
    IReadOnlyList<TaxRuleRequest> Rules);
