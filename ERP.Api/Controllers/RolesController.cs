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
[Route("api/identity/roles")]
public sealed class RolesController : ControllerBase
{
    private readonly RoleService _roleService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public RolesController(RoleService roleService, ICurrentUserProvider currentUserProvider)
    {
        _roleService = roleService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet]
    public async Task<IActionResult> ListRoles(CancellationToken cancellationToken)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null)
        {
            return Unauthorized();
        }

        if (!TryGetCompanyContext(out var companyPublicId, out var companyId))
        {
            return Unauthorized();
        }

        var roles = await _roleService.ListRolesByUserAsync(currentUser.UserId, companyPublicId, companyId, cancellationToken);
        return Ok(roles);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetRole(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyContext(out var companyPublicId, out var companyId))
        {
            return Unauthorized();
        }

        var role = await _roleService.GetRoleAsync(companyPublicId, companyId, id, cancellationToken);
        if (role is null)
        {
            return NotFound();
        }

        return Ok(role);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyContext(out var companyPublicId, out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyPublicId != companyPublicId)
        {
            return Forbid();
        }

        var result = await _roleService.CreateRoleAsync(
            companyPublicId,
            companyId,
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
    public async Task<IActionResult> UpdateRole(long id, UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyContext(out var companyPublicId, out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyPublicId != companyPublicId)
        {
            return Forbid();
        }

        var result = await _roleService.UpdateRoleAsync(
            companyPublicId,
            companyId,
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
