using ERP.Shared.Domain;

namespace ERP.Modules.Accounting.Domain;

public sealed class AccountingAccountTemplate : Entity
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public AccountType AccountType { get; private set; }
    public long? ParentId { get; private set; }
    public bool IsSystemRequired { get; private set; }
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }
    public string Version { get; private set; } = null!;
    public string? SystemRole { get; private set; }

    private AccountingAccountTemplate() { }

    public static AccountingAccountTemplate Create(
        string version,
        string code,
        string name,
        AccountType accountType,
        bool isSystemRequired,
        bool isActive,
        int sortOrder,
        string? systemRole)
        => new()
        {
            Version = version.Trim(),
            Code = code.Trim(),
            Name = name.Trim(),
            AccountType = accountType,
            IsSystemRequired = isSystemRequired,
            IsActive = isActive,
            SortOrder = sortOrder,
            SystemRole = NormalizeSystemRole(systemRole)
        };

    public void UpdateDetails(string name, AccountType accountType, bool isSystemRequired, bool isActive, int sortOrder, string? systemRole)
    {
        Name = name.Trim();
        AccountType = accountType;
        IsSystemRequired = isSystemRequired;
        IsActive = isActive;
        SortOrder = sortOrder;
        SystemRole = NormalizeSystemRole(systemRole);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetParent(long? parentId)
    {
        ParentId = parentId;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string? NormalizeSystemRole(string? systemRole)
        => string.IsNullOrWhiteSpace(systemRole) ? null : systemRole.Trim();
}
