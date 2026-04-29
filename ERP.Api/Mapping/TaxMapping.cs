using ERP.Api.Contracts.Tax;
using ERP.Modules.Tax.Domain;

namespace ERP.Api.Mapping;

public static class TaxMapping
{
    public static TaxGroup? ToTaxGroup(this TaxGroupRequest? request, long companyId)
    {
        if (request is null || request.Rules.Count == 0)
        {
            return null;
        }

        var group = TaxGroup.Create(companyId, request.Name);
        foreach (var rule in request.Rules)
        {
            var tax = Tax.Create(companyId, rule.Code, rule.Name, rule.Rate);
            group.AddRule(tax, rule.Sequence, rule.IsCompound);
        }

        return group;
    }
}
