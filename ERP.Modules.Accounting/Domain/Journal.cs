using ERP.Shared.Domain;

namespace ERP.Modules.Accounting.Domain;

public sealed class Journal : CompanyEntity
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; }

    private Journal() { }

    public static Journal Create(long companyId, string code, string name)
        => new()
        {
            CompanyId = companyId,
            Code = code.Trim(),
            Name = name.Trim(),
            IsActive = true
        };

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
