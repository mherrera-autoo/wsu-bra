using ERP.Shared.Domain;

namespace ERP.Modules.MasterData.Domain;

public sealed class Warehouse : CompanyEntity
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Location { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Warehouse() { }

    public static Warehouse Create(long companyId, string code, string name, string? location = null)
        => new()
        {
            CompanyId = companyId,
            Code = code.Trim(),
            Name = name.Trim(),
            Location = NormalizeNullable(location),
            IsActive = true
        };

    public void Update(string code, string name, bool isActive, string? location = null)
    {
        Code = code.Trim();
        Name = name.Trim();
        Location = NormalizeNullable(location);
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
