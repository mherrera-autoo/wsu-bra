using ERP.Shared.Domain;

namespace ERP.Modules.Tax.Domain;

public sealed class Tax : CompanyEntity
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public decimal Rate { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Tax() { }

    public static Tax Create(long companyId, string code, string name, decimal rate)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        if (rate < 0) throw new ArgumentOutOfRangeException(nameof(rate), "Rate must be >= 0.");
        return new Tax
        {
            CompanyId = companyId,
            Code = code.Trim(),
            Name = name.Trim(),
            Rate = rate
        };
    }

    public void Deactivate() => IsActive = false;
}
