using ERP.Shared.Domain;

namespace ERP.Modules.Identity.Domain;

public sealed class Role : Entity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    public string? RequiredFeatureKey { get; private set; }
    public bool IsSystem { get; private set; } = true;
    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    private Role() { }

public static Role Create(
        string name,
        string? description = null,
        string? requiredFeatureKey = null,
        bool isSystem = true,
        RoleAssignmentScopeType scopeType = RoleAssignmentScopeType.Platform)
        => new()
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            IsActive = true,
            RequiredFeatureKey = requiredFeatureKey?.Trim(),
            IsSystem = isSystem
        };

public void Update(string name, string? description, bool isActive, RoleAssignmentScopeType? scopeType = null)
    {
        Name = name.Trim();
        Description = description?.Trim();
        IsActive = isActive;
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

    public void UpdateMetadata(string? requiredFeatureKey, bool isSystem)
    {
        RequiredFeatureKey = requiredFeatureKey?.Trim();
        IsSystem = isSystem;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Disable();
    }
}
