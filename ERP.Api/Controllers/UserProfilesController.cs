using ERP.Api.Authorization;
using ERP.Api.Contracts.Users;
using ERP.Modules.Users.Application.Services;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/users/profiles")]
public sealed class UserProfilesController : ControllerBase
{
    private readonly UserProfileService _profileService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public UserProfilesController(UserProfileService profileService, ICurrentUserProvider currentUserProvider)
    {
        _profileService = profileService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProfile(CreateUserProfileRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _profileService.CreateAsync(
            userId,
            request.Email,
            request.FirstName,
            request.LastName,
            request.DisplayName,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserProfileSummary>>> ListProfiles(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var profiles = await _profileService.ListAsync(userId, cancellationToken);
        var response = profiles.Select(profile => new UserProfileSummary(
            profile.Id,
            profile.Email,
            profile.FirstName,
            profile.LastName,
            profile.DisplayName,
            profile.IsActive));
        return Ok(response);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<UserProfileSummary>> GetProfile(long id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var profile = await _profileService.GetAsync(id, cancellationToken);
        if (profile is null || profile.UserId != userId)
        {
            return NotFound();
        }

        return Ok(new UserProfileSummary(
            profile.Id,
            profile.Email,
            profile.FirstName,
            profile.LastName,
            profile.DisplayName,
            profile.IsActive));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateProfile(long id, UpdateUserProfileRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _profileService.UpdateAsync(
            userId,
            id,
            request.Email,
            request.FirstName,
            request.LastName,
            request.DisplayName,
            request.IsActive,
            cancellationToken);

        if (!result.Success)
        {
            if (string.Equals(result.Error, "User profile not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteProfile(long id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _profileService.DeleteAsync(userId, id, cancellationToken);
        if (!result.Success)
        {
            if (string.Equals(result.Error, "User profile not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "deleted" });
    }

    private bool TryGetUserId(out long userId)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null)
        {
            userId = default;
            return false;
        }

        userId = currentUser.UserId;
        return true;
    }
}
