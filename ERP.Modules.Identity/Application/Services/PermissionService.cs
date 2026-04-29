using ERP.Modules.Identity.Contracts;
using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.Identity.Application.Services;

public sealed class PermissionService
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PermissionService(IPermissionRepository permissionRepository, IUnitOfWork unitOfWork)
    {
        _permissionRepository = permissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PermissionSummary>> CreatePermissionAsync(
        string code,
        string name,
        string? description,
        CancellationToken cancellationToken = default)
    {
        if (await _permissionRepository.ExistsByCodeAsync(code, cancellationToken: cancellationToken))
        {
            return Result<PermissionSummary>.Fail("Permission code already exists.");
        }

        if (await _permissionRepository.ExistsByNameAsync(name, cancellationToken: cancellationToken))
        {
            return Result<PermissionSummary>.Fail("Permission name already exists.");
        }

        var permission = Permission.Create(code, name, description, ResolveScopeType(code));
        await _permissionRepository.AddAsync(permission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PermissionSummary>.Ok(new PermissionSummary(permission.Id, permission.Code, permission.Name, permission.Description, permission.IsActive));
    }

    public async Task<Result<PermissionSummary>> UpdatePermissionAsync(
        long id,
        string code,
        string name,
        string? description,
        CancellationToken cancellationToken = default)
    {
        var permission = await _permissionRepository.GetByIdAsync(id, cancellationToken);
        if (permission is null)
        {
            return Result<PermissionSummary>.Fail("Permission not found.");
        }

        if (await _permissionRepository.ExistsByCodeAsync(code, id, cancellationToken))
        {
            return Result<PermissionSummary>.Fail("Permission code already exists.");
        }

        if (await _permissionRepository.ExistsByNameAsync(name, id, cancellationToken))
        {
            return Result<PermissionSummary>.Fail("Permission name already exists.");
        }

        permission.Update(code, name, description, ResolveScopeType(code));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PermissionSummary>.Ok(new PermissionSummary(permission.Id, permission.Code, permission.Name, permission.Description, permission.IsActive));
    }

    public async Task<Result> UpdatePermissionStatusAsync(
        long id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var permission = await _permissionRepository.GetByIdAsync(id, cancellationToken);
        if (permission is null)
        {
            return Result.Fail("Permission not found.");
        }

        if (isActive)
        {
            permission.Enable();
        }
        else
        {
            permission.Disable();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> DeletePermissionAsync(long id, CancellationToken cancellationToken = default)
    {
        var permission = await _permissionRepository.GetByIdAsync(id, cancellationToken);
        if (permission is null)
        {
            return Result.Fail("Permission not found.");
        }

        _permissionRepository.Remove(permission);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<PermissionSummary?> GetPermissionAsync(long id, CancellationToken cancellationToken = default)
    {
        var permission = await _permissionRepository.GetByIdAsync(id, cancellationToken);
        if (permission is null)
        {
            return null;
        }

        return new PermissionSummary(permission.Id, permission.Code, permission.Name, permission.Description, permission.IsActive);
    }

    public async Task<IReadOnlyList<PermissionSummary>> ListPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await _permissionRepository.ListAsync(cancellationToken);
        return permissions
            .Select(permission => new PermissionSummary(permission.Id, permission.Code, permission.Name, permission.Description, permission.IsActive))
            .ToArray();
    }

    private static RoleAssignmentScopeType ResolveScopeType(string code)
    {
        if (code.StartsWith("Platform.", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("Admin.", StringComparison.OrdinalIgnoreCase))
        {
            return RoleAssignmentScopeType.Platform;
        }

        if (code.StartsWith("Workspace.", StringComparison.OrdinalIgnoreCase))
        {
            return RoleAssignmentScopeType.Organization;
        }

        return RoleAssignmentScopeType.Company;
    }
}
