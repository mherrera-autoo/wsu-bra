namespace ERP.Modules.Identity.Domain;

public sealed class RolePermission
{
    public long RoleId { get; private set; }
    public long PermissionId { get; private set; }
    public Role Role { get; private set; } = null!;
    public Permission Permission { get; private set; } = null!;

    private RolePermission() { }

    public static RolePermission Create(long roleId, long permissionId)
        => new() { RoleId = roleId, PermissionId = permissionId };
}
