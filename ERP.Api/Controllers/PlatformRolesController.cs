using System;
using ERP.Api.Authorization;
using ERP.Api.Contracts.Identity;
using ERP.Modules.Identity.Contracts;
using ERP.Modules.Identity.Application.Services;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[Tags("Platform")]
[ApiController]
[Authorize]
[RequirePlatformPermission(PermissionKeys.Platform.GlobalIAMManager)]
[Route("api/platform/roles")]
public sealed class PlatformRolesController : ControllerBase
{
    private readonly RoleService _roleService;

    public PlatformRolesController(RoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> ListRoles([FromQuery] Guid? userPublicId, CancellationToken cancellationToken)
    {
        IReadOnlyList<PlatformRoleSummary> roles;
        if (userPublicId.HasValue)
        {
            if (userPublicId.Value == Guid.Empty)
            {
                return BadRequest(new { error = "userPublicId is required." });
            }

            var rolesByUser = await _roleService.ListAllRolesByUserPublicIdAsync(userPublicId.Value, cancellationToken);
            if (rolesByUser is null)
            {
                return NotFound(new { error = "User not found." });
            }

            roles = rolesByUser;
        }
        else
        {
            roles = await _roleService.ListAllRolesAsync(cancellationToken);
        }

        return Ok(roles);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetRole(long id, CancellationToken cancellationToken)
    {
        var role = await _roleService.GetPlatformRoleAsync(id, cancellationToken);
        if (role is null)
        {
            return NotFound();
        }

        return Ok(role);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole(CreatePlatformRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _roleService.CreatePlatformRoleAsync(
            request.Name,
            request.Description,
            request.PermissionIds,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateRole(long id, UpdatePlatformRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _roleService.UpdatePlatformRoleAsync(
            id,
            request.Name,
            request.Description,
            request.IsActive,
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
    public async Task<IActionResult> UpsertUserRoles(
        [FromBody] UpsertPlatformUserRolesRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { error = "Request body is required." });
        }

        if (request.UserPublicId == Guid.Empty)
        {
            return BadRequest(new { error = "userPublicId is required." });
        }

        var result = await _roleService.UpsertPlatformRoleAssignmentsAsync(
            request.UserPublicId,
            request.AddRolePublicIds ?? Array.Empty<Guid>(),
            request.RemoveRolePublicIds ?? Array.Empty<Guid>(),
            cancellationToken);

        if (!result.Success)
        {
            if (string.Equals(result.Error, "User not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }

    [HttpPatch("{id:long}/status")]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateRoleStatusRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var result = await _roleService.UpdatePlatformRoleStatusAsync(id, request.IsActive, cancellationToken);
        if (!result.Success)
        {
            if (string.Equals(result.Error, "Role not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeactivateRole(long id, CancellationToken cancellationToken)
    {
        var result = await _roleService.DeactivatePlatformRoleAsync(id, cancellationToken);
        if (!result.Success)
        {
            if (string.Equals(result.Error, "Role not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "deactivated" });
    }
}
