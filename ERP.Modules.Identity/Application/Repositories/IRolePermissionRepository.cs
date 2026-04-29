using ERP.Modules.Identity.Domain;

namespace ERP.Modules.Identity.Application.Repositories;

public interface IRolePermissionRepository
{
    Task AddRangeAsync(IEnumerable<RolePermission> rolePermissions, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RolePermission>> ListByRoleIdAsync(long roleId, CancellationToken cancellationToken = default);
    void RemoveRange(IEnumerable<RolePermission> rolePermissions);
}
