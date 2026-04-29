using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly ErpDbContext _dbContext;

    public RoleRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(Role role, CancellationToken cancellationToken = default)
        => _dbContext.Roles.AddAsync(role, cancellationToken).AsTask();

    public Task<Role?> GetByIdAsync(long roleId, CancellationToken cancellationToken = default)
        => _dbContext.Roles.FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

    public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        => _dbContext.Roles.FirstOrDefaultAsync(role => role.Name == name.Trim(), cancellationToken);

    public Task<Role?> GetByIdWithPermissionsAsync(RoleAssignmentScopeType scopeType, long roleId, CancellationToken cancellationToken = default)
        => GetByIdWithPermissionsAndScopeAsync(scopeType, roleId, cancellationToken);

    public Task<Role?> GetByNameAsync(RoleAssignmentScopeType scopeType, string name, CancellationToken cancellationToken = default)
        => GetByNameAndScopeAsync(scopeType, name, cancellationToken);

    public async Task<IReadOnlyList<Role>> ListByPublicIdsAsync(
        RoleAssignmentScopeType scopeType,
        IReadOnlyCollection<Guid> rolePublicIds,
        CancellationToken cancellationToken = default)
    {
        if (rolePublicIds.Count == 0)
        {
            return Array.Empty<Role>();
        }

        var roles = await _dbContext.Roles
            .Include(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .Where(role => rolePublicIds.Contains(role.PublicId))
            .ToListAsync(cancellationToken);

        return roles
            .Where(role => IsRoleCompatibleWithScope(role, scopeType))
            .ToArray();
    }

    public async Task<IReadOnlyList<Role>> ListByPublicIdsAsync(
        IReadOnlyCollection<Guid> rolePublicIds,
        CancellationToken cancellationToken = default)
    {
        if (rolePublicIds.Count == 0)
        {
            return Array.Empty<Role>();
        }

        return await _dbContext.Roles
            .Where(role => rolePublicIds.Contains(role.PublicId))
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(RoleAssignmentScopeType scopeType, string name, long? excludeId = null, CancellationToken cancellationToken = default)
        => ExistsByNameAndScopeAsync(scopeType, name, excludeId, cancellationToken);

    public async Task<IReadOnlyList<Role>> ListWithPermissionsAsync(RoleAssignmentScopeType scopeType, CancellationToken cancellationToken = default)
    {
        var roles = await _dbContext.Roles.AsNoTracking()
            .Include(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .OrderBy(role => role.Name)
            .ToListAsync(cancellationToken);

        return roles
            .Where(role => IsRoleCompatibleWithScope(role, scopeType))
            .ToArray();
    }

    public async Task<IReadOnlyList<Role>> ListAllWithPermissionsAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Roles.AsNoTracking()
            .Include(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .OrderBy(role => role.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Role>> ListAssignedWithPermissionsAsync(long userId, CancellationToken cancellationToken = default)
        => await _dbContext.Roles.AsNoTracking()
            .Include(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .Where(role => _dbContext.RoleAssignments.Any(roleAssignment =>
                roleAssignment.UserId == userId
                && roleAssignment.Status == RoleAssignmentStatus.Active
                && roleAssignment.RoleId == role.Id))
            .OrderBy(role => role.Name)
            .ToListAsync(cancellationToken);

    public Task<Role?> GetByIdWithPermissionsAsync(Guid companyPublicId, long roleId, CancellationToken cancellationToken = default)
        => _dbContext.Roles
            .Include(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(role =>
                role.Id == roleId
                && _dbContext.RoleAssignments.Any(roleAssignment =>
                    roleAssignment.RoleId == role.Id
                    && roleAssignment.ScopeType == RoleAssignmentScopeType.Company
                    && roleAssignment.CompanyPublicId == companyPublicId
                    && roleAssignment.Status == RoleAssignmentStatus.Active),
                cancellationToken);

    public Task<Role?> GetByNameAsync(Guid companyPublicId, string name, CancellationToken cancellationToken = default)
        => _dbContext.Roles.FirstOrDefaultAsync(
            r => r.Name == name.Trim()
                && _dbContext.RoleAssignments.Any(roleAssignment =>
                    roleAssignment.RoleId == r.Id
                    && roleAssignment.ScopeType == RoleAssignmentScopeType.Company
                    && roleAssignment.CompanyPublicId == companyPublicId
                    && roleAssignment.Status == RoleAssignmentStatus.Active),
            cancellationToken);

    public Task<bool> ExistsByNameAsync(Guid companyPublicId, string name, long? excludeId = null, CancellationToken cancellationToken = default)
        => _dbContext.Roles.AnyAsync(
            role => role.Name == name.Trim()
                && _dbContext.RoleAssignments.Any(roleAssignment =>
                    roleAssignment.RoleId == role.Id
                    && roleAssignment.ScopeType == RoleAssignmentScopeType.Company
                    && roleAssignment.CompanyPublicId == companyPublicId
                    && roleAssignment.Status == RoleAssignmentStatus.Active)
                && (!excludeId.HasValue || role.Id != excludeId.Value),
            cancellationToken);

    public async Task<IReadOnlyList<Role>> ListAsync(Guid companyPublicId, CancellationToken cancellationToken = default)
        => await _dbContext.Roles.AsNoTracking()
            .Where(role => _dbContext.RoleAssignments.Any(roleAssignment =>
                    roleAssignment.RoleId == role.Id
                    && roleAssignment.ScopeType == RoleAssignmentScopeType.Company
                    && roleAssignment.CompanyPublicId == companyPublicId
                    && roleAssignment.Status == RoleAssignmentStatus.Active))
            .OrderBy(role => role.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Role>> ListWithPermissionsAsync(Guid companyPublicId, CancellationToken cancellationToken = default)
        => await _dbContext.Roles.AsNoTracking()
            .Include(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .Where(role => _dbContext.RoleAssignments.Any(roleAssignment =>
                    roleAssignment.RoleId == role.Id
                    && roleAssignment.ScopeType == RoleAssignmentScopeType.Company
                    && roleAssignment.CompanyPublicId == companyPublicId
                    && roleAssignment.Status == RoleAssignmentStatus.Active))
            .OrderBy(role => role.Name)
            .ToListAsync(cancellationToken);

    private async Task<Role?> GetByIdWithPermissionsAndScopeAsync(
        RoleAssignmentScopeType scopeType,
        long roleId,
        CancellationToken cancellationToken)
    {
        var role = await _dbContext.Roles
            .Include(item => item.RolePermissions)
            .ThenInclude(item => item.Permission)
            .FirstOrDefaultAsync(item => item.Id == roleId, cancellationToken);

        return role is not null && IsRoleCompatibleWithScope(role, scopeType)
            ? role
            : null;
    }

    private async Task<Role?> GetByNameAndScopeAsync(
        RoleAssignmentScopeType scopeType,
        string name,
        CancellationToken cancellationToken)
    {
        var role = await _dbContext.Roles
            .Include(item => item.RolePermissions)
            .ThenInclude(item => item.Permission)
            .FirstOrDefaultAsync(item => item.Name == name.Trim(), cancellationToken);

        return role is not null && IsRoleCompatibleWithScope(role, scopeType)
            ? role
            : null;
    }

    private async Task<bool> ExistsByNameAndScopeAsync(
        RoleAssignmentScopeType scopeType,
        string name,
        long? excludeId,
        CancellationToken cancellationToken)
    {
        var candidates = await _dbContext.Roles
            .Include(item => item.RolePermissions)
            .ThenInclude(item => item.Permission)
            .Where(item => item.Name == name.Trim() && (!excludeId.HasValue || item.Id != excludeId.Value))
            .ToListAsync(cancellationToken);

        return candidates.Any(item => IsRoleCompatibleWithScope(item, scopeType));
    }

    private static bool IsRoleCompatibleWithScope(Role role, RoleAssignmentScopeType scopeType)
    {
        var permissionScopes = role.RolePermissions
            .Select(item => (RoleAssignmentScopeType?)item.Permission?.ScopeType)
            .Where(item => item.HasValue)
            .Select(item => item!.Value)
            .Distinct()
            .ToArray();

        if (permissionScopes.Length == 0)
        {
            return true;
        }

        return permissionScopes.Length == 1 && permissionScopes[0] == scopeType;
    }
}
