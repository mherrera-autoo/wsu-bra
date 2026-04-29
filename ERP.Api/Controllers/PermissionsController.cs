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
[RequirePlatformPermission(PermissionKeys.Platform.GlobalIAMManager)]
[Route("api/identity/permissions")]
public sealed class PermissionsController : ControllerBase
{
    private readonly PermissionService _permissionService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public PermissionsController(PermissionService permissionService, ICurrentUserProvider currentUserProvider)
    {
        _permissionService = permissionService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet]
    public async Task<IActionResult> ListPermissions(CancellationToken cancellationToken)
    {
        if (_currentUserProvider.GetCurrentUser() is null)
        {
            return Unauthorized();
        }

        var permissions = await _permissionService.ListPermissionsAsync(cancellationToken);
        return Ok(permissions);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetPermission(long id, CancellationToken cancellationToken)
    {
        if (_currentUserProvider.GetCurrentUser() is null)
        {
            return Unauthorized();
        }

        var permission = await _permissionService.GetPermissionAsync(id, cancellationToken);
        if (permission is null)
        {
            return NotFound();
        }

        return Ok(permission);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePermission(CreatePermissionRequest request, CancellationToken cancellationToken)
    {
        if (_currentUserProvider.GetCurrentUser() is null)
        {
            return Unauthorized();
        }

        var result = await _permissionService.CreatePermissionAsync(
            request.Code,
            request.Name,
            request.Description,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdatePermission(long id, UpdatePermissionRequest request, CancellationToken cancellationToken)
    {
        if (_currentUserProvider.GetCurrentUser() is null)
        {
            return Unauthorized();
        }

        var result = await _permissionService.UpdatePermissionAsync(
            id,
            request.Code,
            request.Name,
            request.Description,
            cancellationToken);

        if (!result.Success)
        {
            if (string.Equals(result.Error, "Permission not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPatch("{id:long}/status")]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdatePermissionStatusRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        if (_currentUserProvider.GetCurrentUser() is null)
        {
            return Unauthorized();
        }

        var result = await _permissionService.UpdatePermissionStatusAsync(id, request.IsActive, cancellationToken);
        if (!result.Success)
        {
            if (string.Equals(result.Error, "Permission not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }
}
