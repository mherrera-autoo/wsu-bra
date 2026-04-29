using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class RolePermissionRepository : IRolePermissionRepository
{
    private readonly ErpDbContext _dbContext;

    public RolePermissionRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddRangeAsync(IEnumerable<RolePermission> rolePermissions, CancellationToken cancellationToken = default)
    {
        await _dbContext.RolePermissions.AddRangeAsync(rolePermissions, cancellationToken);
    }

    public async Task<IReadOnlyList<RolePermission>> ListByRoleIdAsync(long roleId, CancellationToken cancellationToken = default)
        => await _dbContext.RolePermissions.AsNoTracking()
            .Where(rolePermission => rolePermission.RoleId == roleId)
            .ToListAsync(cancellationToken);

    public void RemoveRange(IEnumerable<RolePermission> rolePermissions)
    {
        _dbContext.RolePermissions.RemoveRange(rolePermissions);
    }
}
