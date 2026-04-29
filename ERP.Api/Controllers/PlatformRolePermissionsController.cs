using System;
using ERP.Api.Authorization;
using ERP.Api.Contracts.Identity;
using ERP.Modules.Identity.Application.Services;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[Tags("Platform")]
[ApiController]
[Authorize]
[RequirePlatformPermission(PermissionKeys.Platform.GlobalIAMManager)]
[Route("api/platform/roles/{roleId:long}/permissions")]
public sealed class PlatformRolePermissionsController : ControllerBase
{
    private readonly RoleService _roleService;

    public PlatformRolePermissionsController(RoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> ListPermissions(long roleId, CancellationToken cancellationToken)
    {
        var role = await _roleService.GetPlatformRoleAsync(roleId, cancellationToken);
        if (role is null)
        {
            return NotFound();
        }

        return Ok(role.Permissions);
    }

    [HttpPost]
    public async Task<IActionResult> AssignPermissions(
        long roleId,
        AssignRolePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { error = "Request body is required." });
        }

        var result = await _roleService.AddPlatformPermissionsAsync(
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

    [HttpPut]
    public async Task<IActionResult> ReplacePermissions(
        long roleId,
        AssignRolePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { error = "Request body is required." });
        }

        var result = await _roleService.ReplacePlatformPermissionsAsync(
            roleId,
            request.PermissionIds ?? Array.Empty<long>(),
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
        if (request is null)
        {
            return BadRequest(new { error = "Request body is required." });
        }

        var result = await _roleService.RemovePlatformPermissionsAsync(
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
}
