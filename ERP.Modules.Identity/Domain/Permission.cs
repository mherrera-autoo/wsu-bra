using ERP.Shared.Domain;

namespace ERP.Modules.Identity.Domain;

public sealed class Permission : Entity
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public RoleAssignmentScopeType ScopeType { get; private set; } = RoleAssignmentScopeType.Company;
    public bool IsActive { get; private set; } = true;
    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    private Permission() { }

    public static Permission Create(
        string code,
        string name,
        string? description = null,
        RoleAssignmentScopeType scopeType = RoleAssignmentScopeType.Company)
        => new()
        {
            Code = code.Trim(),
            Name = name.Trim(),
            Description = description?.Trim(),
            ScopeType = scopeType,
            IsActive = true
        };

    public void Update(string code, string name, string? description, RoleAssignmentScopeType? scopeType = null)
    {
        Code = code.Trim();
        Name = name.Trim();
        Description = description?.Trim();
        if (scopeType.HasValue)
        {
            ScopeType = scopeType.Value;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void Enable()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
