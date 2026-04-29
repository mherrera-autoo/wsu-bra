using System;
using ERP.Api.Authorization;
using ERP.Api.Contracts.Identity;
using ERP.Modules.Identity.Application.Services;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[Tags("Identity")]
[ApiController]
[TenantGuard]
[RequireCompanyPermission(PermissionKeys.Admin.RolesAssign)]
[Route("api/identity/roles/{roleId:long}/permissions")]
public sealed class RolePermissionsController : ControllerBase
{
    private readonly RoleService _roleService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public RolePermissionsController(RoleService roleService, ICurrentUserProvider currentUserProvider)
    {
        _roleService = roleService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpPost]
    public async Task<IActionResult> AssignPermissions(
        long roleId,
        AssignRolePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyContext(out var companyPublicId, out var companyId))
        {
            return Unauthorized();
        }

        var result = await _roleService.AddPermissionsAsync(
            companyPublicId,
            companyId,
            roleId,
            request.PermissionIds,
            cancellationToken);
        if (!result.Success)
        {
            if (string.Equals(result.Error, "Role not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpDelete]
    public async Task<IActionResult> RemovePermissions(
        long roleId,
        [FromBody] AssignRolePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyContext(out var companyPublicId, out var companyId))
        {
            return Unauthorized();
        }

        var result = await _roleService.RemovePermissionsAsync(
            companyPublicId,
            companyId,
            roleId,
            request.PermissionIds,
            cancellationToken);
        if (!result.Success)
        {
            if (string.Equals(result.Error, "Role not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    private bool TryGetCompanyContext(out Guid companyPublicId, out long companyId)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null || !currentUser.CompanyPublicId.HasValue)
        {
            companyId = 0;
            companyPublicId = Guid.Empty;
            return false;
        }

        companyPublicId = currentUser.CompanyPublicId.Value;
        companyId = currentUser.CompanyId;
        return true;
    }
}
