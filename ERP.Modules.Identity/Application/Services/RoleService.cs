using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Modules.Identity.Contracts;
using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Domain;
using ERP.Modules.MasterData.Contracts;
using ERP.Shared.Application;

namespace ERP.Modules.Identity.Application.Services;

public sealed class RoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IRoleAssignmentRepository _roleAssignmentRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;

    public RoleService(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IRolePermissionRepository rolePermissionRepository,
        IRoleAssignmentRepository roleAssignmentRepository,
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _roleAssignmentRepository = roleAssignmentRepository;
        _userRepository = userRepository;
        _companyRepository = companyRepository;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RoleSummary>> CreateRoleAsync(
        Guid companyPublicId,
        long companyId,
        string name,
        string? description,
        IReadOnlyCollection<long>? permissionIds,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            if (await _roleRepository.ExistsByNameAsync(RoleAssignmentScopeType.Company, name, cancellationToken: token))
            {
                return Result<RoleSummary>.Fail("Role name already exists.");
            }

            var permissions = await LoadPermissionsAsync(permissionIds, token);
            if (!permissions.Success)
            {
                return Result<RoleSummary>.Fail(permissions.Error ?? "Permission validation failed.");
            }

            var role = Role.Create(
                name,
                description,
                requiredFeatureKey: null,
                isSystem: false,
                scopeType: RoleAssignmentScopeType.Company);
            await _roleRepository.AddAsync(role, token);
            await _unitOfWork.SaveChangesAsync(token);

            if (permissions.Value!.Count > 0)
            {
                var rolePermissions = permissions.Value
                    .Select(permission => RolePermission.Create(role.Id, permission.Id))
                    .ToArray();

                await _rolePermissionRepository.AddRangeAsync(rolePermissions, token);
                await _unitOfWork.SaveChangesAsync(token);
            }

            return Result<RoleSummary>.Ok(MapRole(role, companyPublicId, companyId, permissions.Value));
        }, cancellationToken);
    }

    public async Task<Result<PlatformRoleSummary>> CreatePlatformRoleAsync(
        string name,
        string? description,
        IReadOnlyCollection<long>? permissionIds,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            if (await _roleRepository.ExistsByNameAsync(RoleAssignmentScopeType.Platform, name, cancellationToken: token))
            {
                return Result<PlatformRoleSummary>.Fail("Role name already exists.");
            }

            var permissions = await LoadPermissionsAsync(permissionIds, token);
            if (!permissions.Success)
            {
                return Result<PlatformRoleSummary>.Fail(permissions.Error ?? "Permission validation failed.");
            }

            var role = Role.Create(
                name,
                description,
                requiredFeatureKey: null,
                isSystem: false,
                scopeType: RoleAssignmentScopeType.Platform);
            await _roleRepository.AddAsync(role, token);
            await _unitOfWork.SaveChangesAsync(token);

            if (permissions.Value!.Count > 0)
            {
                var rolePermissions = permissions.Value
                    .Select(permission => RolePermission.Create(role.Id, permission.Id))
                    .ToArray();

                await _rolePermissionRepository.AddRangeAsync(rolePermissions, token);
                await _unitOfWork.SaveChangesAsync(token);
            }

            return Result<PlatformRoleSummary>.Ok(MapPlatformRole(role, permissions.Value));
        }, cancellationToken);
    }

    public async Task<Result<RoleSummary>> UpdateRoleAsync(
        Guid companyPublicId,
        long companyId,
        long roleId,
        string name,
        string? description,
        bool isActive,
        IReadOnlyCollection<long>? permissionIds,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var role = await _roleRepository.GetByIdWithPermissionsAsync(companyPublicId, roleId, token);
            if (role is null)
            {
                return Result<RoleSummary>.Fail("Role not found.");
            }

            if (await _roleRepository.ExistsByNameAsync(RoleAssignmentScopeType.Company, name, role.Id, token))
            {
                return Result<RoleSummary>.Fail("Role name already exists.");
            }

            IReadOnlyList<Permission>? permissionsOverride = null;
            if (permissionIds is not null)
            {
                var permissions = await LoadPermissionsAsync(permissionIds, token);
                if (!permissions.Success)
                {
                    return Result<RoleSummary>.Fail(permissions.Error ?? "Permission validation failed.");
                }

                permissionsOverride = permissions.Value;
            }

            role.Update(name, description, isActive);

            if (permissionIds is not null)
            {
                var currentIds = role.RolePermissions.Select(rolePermission => rolePermission.PermissionId).ToHashSet();
                var targetIds = permissionsOverride!.Select(permission => permission.Id).ToHashSet();

                var toRemove = role.RolePermissions
                    .Where(rolePermission => !targetIds.Contains(rolePermission.PermissionId))
                    .ToArray();

                if (toRemove.Length > 0)
                {
                    _rolePermissionRepository.RemoveRange(toRemove);
                }

                var toAdd = targetIds.Except(currentIds)
                    .Select(permissionId => RolePermission.Create(role.Id, permissionId))
                    .ToArray();

                if (toAdd.Length > 0)
                {
                    await _rolePermissionRepository.AddRangeAsync(toAdd, token);
                }
            }

            await _unitOfWork.SaveChangesAsync(token);

            return Result<RoleSummary>.Ok(MapRole(role, companyPublicId, companyId, permissionsOverride));
        }, cancellationToken);
    }

    public async Task<Result<PlatformRoleSummary>> UpdatePlatformRoleAsync(
        long roleId,
        string name,
        string? description,
        bool isActive,
        IReadOnlyCollection<long>? permissionIds,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var role = await _roleRepository.GetByIdWithPermissionsAsync(RoleAssignmentScopeType.Platform, roleId, token);
            if (role is null)
            {
                return Result<PlatformRoleSummary>.Fail("Role not found.");
            }

            if (await _roleRepository.ExistsByNameAsync(RoleAssignmentScopeType.Platform, name, role.Id, token))
            {
                return Result<PlatformRoleSummary>.Fail("Role name already exists.");
            }

            IReadOnlyList<Permission>? permissionsOverride = null;
            if (permissionIds is not null)
            {
                var permissions = await LoadPermissionsAsync(permissionIds, token);
                if (!permissions.Success)
                {
                    return Result<PlatformRoleSummary>.Fail(permissions.Error ?? "Permission validation failed.");
                }

                permissionsOverride = permissions.Value;
            }

            role.Update(name, description, isActive, RoleAssignmentScopeType.Platform);

            if (permissionIds is not null)
            {
                var currentIds = role.RolePermissions.Select(rolePermission => rolePermission.PermissionId).ToHashSet();
                var targetIds = permissionsOverride!.Select(permission => permission.Id).ToHashSet();

                var toRemove = role.RolePermissions
                    .Where(rolePermission => !targetIds.Contains(rolePermission.PermissionId))
                    .ToArray();

                if (toRemove.Length > 0)
                {
                    _rolePermissionRepository.RemoveRange(toRemove);
                }

                var toAdd = targetIds.Except(currentIds)
                    .Select(permissionId => RolePermission.Create(role.Id, permissionId))
                    .ToArray();

                if (toAdd.Length > 0)
                {
                    await _rolePermissionRepository.AddRangeAsync(toAdd, token);
                }
            }

            await _unitOfWork.SaveChangesAsync(token);

            return Result<PlatformRoleSummary>.Ok(MapPlatformRole(role, permissionsOverride));
        }, cancellationToken);
    }

    public async Task<Result> UpdateRoleStatusAsync(
        Guid companyPublicId,
        long roleId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdWithPermissionsAsync(companyPublicId, roleId, cancellationToken);
        if (role is null)
        {
            return Result.Fail("Role not found.");
        }

        if (isActive)
        {
            role.Enable();
        }
        else
        {
            role.Disable();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> UpdatePlatformRoleStatusAsync(
        long roleId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
        if (role is null)
        {
            return Result.Fail("Role not found.");
        }

        if (isActive)
        {
            role.Enable();
        }
        else
        {
            role.Disable();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> DeactivateRoleAsync(Guid companyPublicId, long roleId, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdWithPermissionsAsync(companyPublicId, roleId, cancellationToken);
        if (role is null)
        {
            return Result.Fail("Role not found.");
        }

        role.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> DeactivatePlatformRoleAsync(long roleId, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdWithPermissionsAsync(RoleAssignmentScopeType.Platform, roleId, cancellationToken);
        if (role is null)
        {
            return Result.Fail("Role not found.");
        }

        role.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<RoleSummary?> GetRoleAsync(Guid companyPublicId, long companyId, long roleId, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdWithPermissionsAsync(companyPublicId, roleId, cancellationToken);
        if (role is null)
        {
            return null;
        }

        return MapRole(role, companyPublicId, companyId);
    }

    public async Task<PlatformRoleSummary?> GetPlatformRoleAsync(long roleId, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdWithPermissionsAsync(RoleAssignmentScopeType.Platform, roleId, cancellationToken);
        if (role is null)
        {
            return null;
        }

        return MapPlatformRole(role);
    }

    public async Task<IReadOnlyList<RoleSummary>> ListRolesAsync(Guid companyPublicId, long companyId, CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.ListWithPermissionsAsync(companyPublicId, cancellationToken);
        return roles.Select(role => MapRole(role, companyPublicId, companyId)).ToArray();
    }

    public async Task<IReadOnlyList<RoleSummary>> ListRolesByUserAsync(
        long userId,
        Guid companyPublicId,
        long companyId,
        CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.ListAllWithPermissionsAsync(cancellationToken);
        var assignedRoles = await _roleRepository.ListAssignedWithPermissionsAsync(userId, cancellationToken);
        var assignedRoleIds = assignedRoles.Select(role => role.Id).ToHashSet();

        return roles
            .Where(role => RoleHasPermissionScope(role, RoleAssignmentScopeType.Platform)
                || RoleHasPermissionScope(role, RoleAssignmentScopeType.Organization))
            .Select(role => MapRole(role, companyPublicId, companyId, isAssigned: assignedRoleIds.Contains(role.Id)))
            .ToArray();
    }

    public async Task<IReadOnlyList<PlatformRoleSummary>> ListPlatformRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.ListWithPermissionsAsync(RoleAssignmentScopeType.Platform, cancellationToken);
        return roles.Select(role => MapPlatformRole(role)).ToArray();
    }

    public async Task<IReadOnlyList<PlatformRoleSummary>> ListAllRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.ListAllWithPermissionsAsync(cancellationToken);
        return roles.Select(role => MapPlatformRole(role)).ToArray();
    }

    public async Task<IReadOnlyList<PlatformRoleSummary>?> ListAllRolesByUserPublicIdAsync(
        Guid userPublicId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByPublicIdAsync(userPublicId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var roles = await _roleRepository.ListAllWithPermissionsAsync(cancellationToken);
        var assignedRoles = await _roleRepository.ListAssignedWithPermissionsAsync(user.Id, cancellationToken);
        var assignedRoleIds = assignedRoles.Select(role => role.Id).ToHashSet();

        return roles
            .Select(role => MapPlatformRole(role, isAssignedToUser: assignedRoleIds.Contains(role.Id)))
            .ToArray();
    }

    public async Task<Result> UpsertPlatformRoleAssignmentsAsync(
        Guid userPublicId,
        IReadOnlyCollection<Guid> addRolePublicIds,
        IReadOnlyCollection<Guid> removeRolePublicIds,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var user = await _userRepository.GetByPublicIdAsync(userPublicId, token);
            if (user is null)
            {
                return Result.Fail("User not found.");
            }

            var addSet = addRolePublicIds
                .Where(rolePublicId => rolePublicId != Guid.Empty)
                .ToHashSet();
            var removeSet = removeRolePublicIds
                .Where(rolePublicId => rolePublicId != Guid.Empty)
                .ToHashSet();

            if (addSet.Overlaps(removeSet))
            {
                return Result.Fail("A role cannot be in both add and remove lists.");
            }

            var targetRolePublicIds = addSet
                .Concat(removeSet)
                .ToHashSet();

            if (targetRolePublicIds.Count == 0)
            {
                return Result.Ok();
            }

            var roles = await _roleRepository.ListByPublicIdsAsync(targetRolePublicIds.ToArray(), token);
            var rolesByPublicId = roles.ToDictionary(role => role.PublicId);

            var missingRolePublicIds = targetRolePublicIds
                .Where(rolePublicId => !rolesByPublicId.ContainsKey(rolePublicId))
                .Select(rolePublicId => rolePublicId.ToString())
                .ToArray();

            if (missingRolePublicIds.Length > 0)
            {
                return Result.Fail($"Some roles were not found: {string.Join(", ", missingRolePublicIds)}");
            }

            var existingAssignments = await _roleAssignmentRepository.GetByUserAsync(user.Id, token);

            foreach (var rolePublicId in addSet)
            {
                var role = rolesByPublicId[rolePublicId];
                const RoleAssignmentScopeType assignmentScope = RoleAssignmentScopeType.Platform;
                var context = await ResolveScopeContextAsync(assignmentScope, token);
                if (!context.Success)
                {
                    return Result.Fail(context.Error!);
                }

                var assignment = existingAssignments.FirstOrDefault(existing =>
                    existing.RoleId == role.Id
                    && existing.ScopeType == assignmentScope
                    && existing.OrganizationId == context.Value.OrganizationId
                    && existing.CompanyPublicId == context.Value.CompanyPublicId);

                if (assignment is null)
                {
                    await _roleAssignmentRepository.AddAsync(
                        CreateAssignment(user.Id, role.Id, assignmentScope, context.Value.OrganizationId, context.Value.CompanyPublicId),
                        token);
                    continue;
                }

                if (assignment.Status == RoleAssignmentStatus.Inactive)
                {
                    assignment.Activate();
                    await _roleAssignmentRepository.UpdateAsync(assignment, token);
                }
            }

            foreach (var rolePublicId in removeSet)
            {
                var role = rolesByPublicId[rolePublicId];
                const RoleAssignmentScopeType assignmentScope = RoleAssignmentScopeType.Platform;
                var context = await ResolveScopeContextAsync(assignmentScope, token);
                if (!context.Success)
                {
                    return Result.Fail(context.Error!);
                }

                var activeAssignments = existingAssignments
                    .Where(existing =>
                        existing.RoleId == role.Id
                        && existing.ScopeType == assignmentScope
                        && existing.OrganizationId == context.Value.OrganizationId
                        && existing.CompanyPublicId == context.Value.CompanyPublicId
                        && existing.Status == RoleAssignmentStatus.Active)
                    .ToArray();

                foreach (var assignment in activeAssignments)
                {
                    assignment.Deactivate();
                    await _roleAssignmentRepository.UpdateAsync(assignment, token);
                }
            }

            await _unitOfWork.SaveChangesAsync(token);
            return Result.Ok();
        }, cancellationToken);
    }

    public async Task<Result<RoleSummary>> AddPermissionsAsync(
        Guid companyPublicId,
        long companyId,
        long roleId,
        IReadOnlyCollection<long> permissionIds,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var role = await _roleRepository.GetByIdWithPermissionsAsync(companyPublicId, roleId, token);
            if (role is null)
            {
                return Result<RoleSummary>.Fail("Role not found.");
            }

            var permissionsResult = await LoadPermissionsAsync(permissionIds, token);
            if (!permissionsResult.Success)
            {
                return Result<RoleSummary>.Fail(permissionsResult.Error ?? "Permission validation failed.");
            }

            var permissions = permissionsResult.Value ?? Array.Empty<Permission>();
            if (permissions.Count == 0)
            {
                return Result<RoleSummary>.Ok(MapRole(role, companyPublicId, companyId));
            }

            var existingIds = role.RolePermissions.Select(rolePermission => rolePermission.PermissionId).ToHashSet();
            var toAdd = permissions
                .Where(permission => !existingIds.Contains(permission.Id))
                .Select(permission => RolePermission.Create(role.Id, permission.Id))
                .ToArray();

            if (toAdd.Length > 0)
            {
                await _rolePermissionRepository.AddRangeAsync(toAdd, token);
                foreach (var rolePermission in toAdd)
                {
                    role.RolePermissions.Add(rolePermission);
                }
            }

            await _unitOfWork.SaveChangesAsync(token);

            var updatedPermissions = role.RolePermissions
                .Select(rolePermission => rolePermission.Permission)
                .Where(permission => permission is not null)
                .Concat(permissions)
                .GroupBy(permission => permission.Id)
                .Select(group => group.First()!)
                .ToArray();

            return Result<RoleSummary>.Ok(MapRole(role, companyPublicId, companyId, updatedPermissions));
        }, cancellationToken);
    }

    public async Task<Result<PlatformRoleSummary>> AddPlatformPermissionsAsync(
        long roleId,
        IReadOnlyCollection<long> permissionIds,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var role = await _roleRepository.GetByIdWithPermissionsAsync(RoleAssignmentScopeType.Platform, roleId, token);
            if (role is null)
            {
                return Result<PlatformRoleSummary>.Fail("Role not found.");
            }

            var permissionsResult = await LoadPermissionsAsync(permissionIds, token);
            if (!permissionsResult.Success)
            {
                return Result<PlatformRoleSummary>.Fail(permissionsResult.Error ?? "Permission validation failed.");
            }

            var permissions = permissionsResult.Value ?? Array.Empty<Permission>();
            if (permissions.Count == 0)
            {
                return Result<PlatformRoleSummary>.Ok(MapPlatformRole(role));
            }

            var existingIds = role.RolePermissions.Select(rolePermission => rolePermission.PermissionId).ToHashSet();
            var toAdd = permissions
                .Where(permission => !existingIds.Contains(permission.Id))
                .Select(permission => RolePermission.Create(role.Id, permission.Id))
                .ToArray();

            if (toAdd.Length > 0)
            {
                await _rolePermissionRepository.AddRangeAsync(toAdd, token);
                foreach (var rolePermission in toAdd)
                {
                    role.RolePermissions.Add(rolePermission);
                }
            }

            await _unitOfWork.SaveChangesAsync(token);

            var updatedPermissions = role.RolePermissions
                .Select(rolePermission => rolePermission.Permission)
                .Where(permission => permission is not null)
                .Concat(permissions)
                .GroupBy(permission => permission.Id)
                .Select(group => group.First()!)
                .ToArray();

            return Result<PlatformRoleSummary>.Ok(MapPlatformRole(role, updatedPermissions));
        }, cancellationToken);
    }

    public async Task<Result<RoleSummary>> RemovePermissionsAsync(
        Guid companyPublicId,
        long companyId,
        long roleId,
        IReadOnlyCollection<long> permissionIds,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var role = await _roleRepository.GetByIdWithPermissionsAsync(companyPublicId, roleId, token);
            if (role is null)
            {
                return Result<RoleSummary>.Fail("Role not found.");
            }

            var permissionsResult = await LoadPermissionsAsync(permissionIds, token);
            if (!permissionsResult.Success)
            {
                return Result<RoleSummary>.Fail(permissionsResult.Error ?? "Permission validation failed.");
            }

            var targetIds = permissionsResult.Value?.Select(permission => permission.Id).ToHashSet()
                ?? new HashSet<long>();

            var toRemove = role.RolePermissions
                .Where(rolePermission => targetIds.Contains(rolePermission.PermissionId))
                .ToArray();

            if (toRemove.Length > 0)
            {
                _rolePermissionRepository.RemoveRange(toRemove);
                foreach (var rolePermission in toRemove)
                {
                    role.RolePermissions.Remove(rolePermission);
                }
            }

            await _unitOfWork.SaveChangesAsync(token);

            var updatedPermissions = role.RolePermissions
                .Select(rolePermission => rolePermission.Permission)
                .Where(permission => permission is not null)
                .ToArray();

            return Result<RoleSummary>.Ok(MapRole(role, companyPublicId, companyId, updatedPermissions));
        }, cancellationToken);
    }

    public async Task<Result<PlatformRoleSummary>> RemovePlatformPermissionsAsync(
        long roleId,
        IReadOnlyCollection<long> permissionIds,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var role = await _roleRepository.GetByIdWithPermissionsAsync(RoleAssignmentScopeType.Platform, roleId, token);
            if (role is null)
            {
                return Result<PlatformRoleSummary>.Fail("Role not found.");
            }

            var permissionsResult = await LoadPermissionsAsync(permissionIds, token);
            if (!permissionsResult.Success)
            {
                return Result<PlatformRoleSummary>.Fail(permissionsResult.Error ?? "Permission validation failed.");
            }

            var targetIds = permissionsResult.Value?.Select(permission => permission.Id).ToHashSet()
                ?? new HashSet<long>();

            var toRemove = role.RolePermissions
                .Where(rolePermission => targetIds.Contains(rolePermission.PermissionId))
                .ToArray();

            if (toRemove.Length > 0)
            {
                _rolePermissionRepository.RemoveRange(toRemove);
                foreach (var rolePermission in toRemove)
                {
                    role.RolePermissions.Remove(rolePermission);
                }
            }

            await _unitOfWork.SaveChangesAsync(token);

            var updatedPermissions = role.RolePermissions
                .Select(rolePermission => rolePermission.Permission)
                .Where(permission => permission is not null)
                .ToArray();

            return Result<PlatformRoleSummary>.Ok(MapPlatformRole(role, updatedPermissions));
        }, cancellationToken);
    }

    public async Task<Result<PlatformRoleSummary>> ReplacePlatformPermissionsAsync(
        long roleId,
        IReadOnlyCollection<long> permissionIds,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var role = await _roleRepository.GetByIdWithPermissionsAsync(RoleAssignmentScopeType.Platform, roleId, token);
            if (role is null)
            {
                return Result<PlatformRoleSummary>.Fail("Role not found.");
            }

            var permissionsResult = await LoadPermissionsAsync(permissionIds, token);
            if (!permissionsResult.Success)
            {
                return Result<PlatformRoleSummary>.Fail(permissionsResult.Error ?? "Permission validation failed.");
            }

            var targetPermissions = permissionsResult.Value ?? Array.Empty<Permission>();
            var targetIds = targetPermissions.Select(permission => permission.Id).ToHashSet();
            var currentIds = role.RolePermissions.Select(rolePermission => rolePermission.PermissionId).ToHashSet();

            var toRemove = role.RolePermissions
                .Where(rolePermission => !targetIds.Contains(rolePermission.PermissionId))
                .ToArray();

            if (toRemove.Length > 0)
            {
                _rolePermissionRepository.RemoveRange(toRemove);
                foreach (var rolePermission in toRemove)
                {
                    role.RolePermissions.Remove(rolePermission);
                }
            }

            var toAdd = targetIds.Except(currentIds)
                .Select(permissionId => RolePermission.Create(role.Id, permissionId))
                .ToArray();

            if (toAdd.Length > 0)
            {
                await _rolePermissionRepository.AddRangeAsync(toAdd, token);
                foreach (var rolePermission in toAdd)
                {
                    role.RolePermissions.Add(rolePermission);
                }
            }

            await _unitOfWork.SaveChangesAsync(token);

            var updatedPermissions = role.RolePermissions
                .Select(rolePermission => rolePermission.Permission)
                .Where(permission => permission is not null)
                .Concat(targetPermissions)
                .GroupBy(permission => permission.Id)
                .Select(group => group.First()!)
                .ToArray();

            return Result<PlatformRoleSummary>.Ok(MapPlatformRole(role, updatedPermissions));
        }, cancellationToken);
    }

    private async Task<Result<(long? OrganizationId, Guid? CompanyPublicId)>> ResolveScopeContextAsync(
        RoleAssignmentScopeType scopeType,
        CancellationToken cancellationToken)
    {
        return scopeType switch
        {
            RoleAssignmentScopeType.Platform => Result<(long?, Guid?)>.Ok((null, null)),
            RoleAssignmentScopeType.Company => _tenantContext.CompanyPublicId.HasValue
                ? Result<(long?, Guid?)>.Ok((null, _tenantContext.CompanyPublicId.Value))
                : Result<(long?, Guid?)>.Fail("Company context is required for company-scoped roles."),
            RoleAssignmentScopeType.Organization => await ResolveOrganizationContextAsync(cancellationToken),
            _ => Result<(long?, Guid?)>.Fail("Unsupported role scope.")
        };
    }

    private async Task<Result<(long? OrganizationId, Guid? CompanyPublicId)>> ResolveOrganizationContextAsync(
        CancellationToken cancellationToken)
    {
        if (_tenantContext.CompanyId.HasValue && _tenantContext.CompanyId.Value > 0)
        {
            var organizationId = await _companyRepository.GetOrganizationIdAsync(_tenantContext.CompanyId.Value, cancellationToken);
            if (organizationId.HasValue)
            {
                return Result<(long?, Guid?)>.Ok((organizationId.Value, null));
            }
        }

        return Result<(long?, Guid?)>.Fail("Organization context is required for organization-scoped roles.");
    }

    private static RoleAssignment CreateAssignment(
        long userId,
        long roleId,
        RoleAssignmentScopeType scopeType,
        long? organizationId,
        Guid? companyPublicId)
    {
        return scopeType switch
        {
            RoleAssignmentScopeType.Platform => RoleAssignment.CreatePlatform(userId, roleId),
            RoleAssignmentScopeType.Organization when organizationId.HasValue =>
                RoleAssignment.CreateOrganization(userId, roleId, organizationId.Value),
            RoleAssignmentScopeType.Company when companyPublicId.HasValue =>
                RoleAssignment.CreateCompany(userId, roleId, companyPublicId.Value),
            _ => throw new InvalidOperationException("Role scope context is invalid.")
        };
    }

    private static bool RoleHasPermissionScope(Role role, RoleAssignmentScopeType scopeType)
        => role.RolePermissions
            .Any(rolePermission => rolePermission.Permission is not null && rolePermission.Permission.ScopeType == scopeType);

    private async Task<Result<IReadOnlyList<Permission>>> LoadPermissionsAsync(
        IReadOnlyCollection<long>? permissionIds,
        CancellationToken cancellationToken)
    {
        if (permissionIds is null || permissionIds.Count == 0)
        {
            return Result<IReadOnlyList<Permission>>.Ok(Array.Empty<Permission>());
        }

        var distinctIds = permissionIds.Distinct().ToArray();
        var permissions = await _permissionRepository.ListByIdsAsync(distinctIds, cancellationToken);
        if (permissions.Count != distinctIds.Length)
        {
            return Result<IReadOnlyList<Permission>>.Fail("One or more permissions were not found.");
        }

        return Result<IReadOnlyList<Permission>>.Ok(permissions);
    }

    private static RoleSummary MapRole(
        Role role,
        Guid companyPublicId,
        long companyId,
        IReadOnlyCollection<Permission>? permissionsOverride = null,
        bool isAssigned = false)
    {
        var permissions = permissionsOverride ?? role.RolePermissions
            .Select(rolePermission => rolePermission.Permission)
            .Where(permission => permission is not null)
            .ToArray();

        var summaries = permissions
            .Select(permission => new PermissionSummary(permission.Id, permission.Code, permission.Name, permission.Description, permission.IsActive))
            .ToArray();

        return new RoleSummary(role.Id, companyPublicId, companyId, role.Name, role.Description, role.IsActive, summaries, isAssigned);
    }

    private static PlatformRoleSummary MapPlatformRole(
        Role role,
        IReadOnlyCollection<Permission>? permissionsOverride = null,
        bool isAssignedToUser = false)
    {
        var permissions = permissionsOverride ?? role.RolePermissions
            .Select(rolePermission => rolePermission.Permission)
            .Where(permission => permission is not null)
            .ToArray();

        var summaries = permissions
            .Select(permission => new PermissionSummary(permission.Id, permission.Code, permission.Name, permission.Description, permission.IsActive))
            .ToArray();

        return new PlatformRoleSummary(role.PublicId, role.Name, role.Description, role.IsActive, summaries, isAssignedToUser);
    }
}
