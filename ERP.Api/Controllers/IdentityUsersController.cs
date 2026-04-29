using ERP.Modules.Identity.Application.Services;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/identity/users")]
[Authorize]
public sealed class IdentityUsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public IdentityUsersController(UserService userService, ICurrentUserProvider currentUserProvider)
    {
        _userService = userService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        if (_currentUserProvider.GetCurrentUser() is null)
        {
            return Unauthorized();
        }

        var users = await _userService.GetUsersAsync(cancellationToken);
        return Ok(users);
    }
}
