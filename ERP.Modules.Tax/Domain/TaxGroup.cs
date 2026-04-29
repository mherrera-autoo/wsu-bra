using ERP.Shared.Domain;

namespace ERP.Modules.Tax.Domain;

public sealed class TaxGroup : CompanyEntity
{
    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;
    public List<TaxRule> Rules { get; private set; } = new();

    private TaxGroup() { }

    public static TaxGroup Create(long companyId, string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        return new TaxGroup { CompanyId = companyId, Name = name.Trim() };
    }

    public TaxRule AddRule(Tax tax, int sequence = 1, bool isCompound = false)
    {
        if (tax is null) throw new ArgumentNullException(nameof(tax));
        return AddRule(tax.Id, tax.Rate, sequence, isCompound);
    }

    public TaxRule AddRule(long taxId, decimal rate, int sequence = 1, bool isCompound = false)
    {
        if (rate < 0) throw new ArgumentOutOfRangeException(nameof(rate));
        var rule = TaxRule.Create(CompanyId, Id, taxId, rate, sequence, isCompound);
        Rules.Add(rule);
        return rule;
    }
}
