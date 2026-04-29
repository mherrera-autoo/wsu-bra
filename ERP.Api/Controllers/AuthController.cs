using ERP.Api.Authorization;
using System.Security.Claims;
using ERP.Api.Authentication;
using ERP.Api.Contracts.Auth;
using ERP.Modules.Identity.Application.Commands;
using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Application.Security;
using ERP.Modules.Identity.Application.Services;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace ERP.Api.Controllers;

[ApiController]
[Route("auth")]
[EnableCors("SpaAuthCors")]
public sealed class AuthController : ControllerBase
{
    private const string RefreshCookieName = "rt";
    private const string SpaXsrfCookieName = "XSRF-TOKEN";
    private const int RefreshCookieDays = 30;

    private readonly IdentityService _identityService;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly PasswordResetService _passwordResetService;
    private readonly IAntiforgery _antiforgery;
    private readonly AuthHttpOptions _authHttpOptions;
    private readonly IIdentityEndpointRateLimiter _identityEndpointRateLimiter;

    public AuthController(
        IdentityService identityService,
        IUserSessionRepository userSessionRepository,
        ICurrentUserProvider currentUserProvider,
        PasswordResetService passwordResetService,
        IAntiforgery antiforgery,
        IOptions<AuthHttpOptions> authHttpOptions,
        IIdentityEndpointRateLimiter identityEndpointRateLimiter)
    {
        _identityService = identityService;
        _userSessionRepository = userSessionRepository;
        _currentUserProvider = currentUserProvider;
        _passwordResetService = passwordResetService;
        _antiforgery = antiforgery;
        _authHttpOptions = authHttpOptions.Value;
        _identityEndpointRateLimiter = identityEndpointRateLimiter;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        if (HasAuthorizationHeader())
        {
            return BadRequest(CreateAuthEndpointAuthorizationProblem());
        }

        var normalizedEmail = request.Email?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return Unauthorized(CreateUnauthorizedProblem());
        }

        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var loginKey = normalizedEmail;
        if (!_identityEndpointRateLimiter.TryConsumeLogin(clientIp, loginKey, out var loginRetryAfter))
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, CreateRateLimitProblem(loginRetryAfter));
        }

        var deviceInfo = FormatDeviceInfo(request.DeviceInfo);
        var result = await _identityService.LoginAsync(normalizedEmail, request.Password, deviceInfo, cancellationToken);
        if (!result.Success || result.Value is null || string.IsNullOrWhiteSpace(result.Value.RefreshToken))
        {
            return Unauthorized(CreateUnauthorizedProblem());
        }

        Response.Cookies.Append(RefreshCookieName, result.Value.RefreshToken, CreateRefreshCookieOptions());
        var tokens = GetAndStoreAntiforgeryTokensAsAnonymous();
        AppendSpaXsrfTokenCookie(tokens.RequestToken);

        return Ok(ToAuthResponse(result.Value));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        if (HasAuthorizationHeader())
        {
            return BadRequest(CreateAuthEndpointAuthorizationProblem());
        }

        if (!HttpMethods.IsOptions(Request.Method))
        {
            try
            {
                await ValidateAntiforgeryAsAnonymousAsync();
            }
            catch (AntiforgeryValidationException)
            {
                return BadRequest(CreateCsrfProblem());
            }
        }

        if (!Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized(CreateUnauthorizedProblem());
        }

        var refreshResult = await _identityService.RefreshTokenAsync(refreshToken, cancellationToken);
        if (refreshResult.Status == RefreshTokenStatus.RateLimited)
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, CreateRateLimitProblem(refreshResult.RetryAfter));
        }

        if (!refreshResult.IsSuccess || refreshResult.Auth is null)
        {
            ClearAuthCookies();
            return Unauthorized(CreateUnauthorizedProblem());
        }

        if (refreshResult.Status == RefreshTokenStatus.Success)
        {
            if (string.IsNullOrWhiteSpace(refreshResult.Auth.RefreshToken))
            {
                ClearAuthCookies();
                return Unauthorized(CreateUnauthorizedProblem());
            }

            Response.Cookies.Append(RefreshCookieName, refreshResult.Auth.RefreshToken, CreateRefreshCookieOptions());
        }

        var refreshTokens = GetAndStoreAntiforgeryTokensAsAnonymous();
        AppendSpaXsrfTokenCookie(refreshTokens.RequestToken);

        return Ok(ToAuthResponse(refreshResult.Auth));
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (HasAuthorizationHeader())
        {
            return BadRequest(CreateAuthEndpointAuthorizationProblem());
        }

        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        if (!_identityEndpointRateLimiter.TryConsumeLogout(clientIp, out var logoutRetryAfter))
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, CreateRateLimitProblem(logoutRetryAfter));
        }

        if (!HttpMethods.IsOptions(Request.Method))
        {
            try
            {
                await ValidateAntiforgeryAsAnonymousAsync();
            }
            catch (AntiforgeryValidationException)
            {
                return BadRequest(CreateCsrfProblem());
            }
        }

        if (Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken) && !string.IsNullOrWhiteSpace(refreshToken))
        {
            await _identityService.RevokeSessionAsync(refreshToken, sessionId: null, cancellationToken);
        }

        ClearAuthCookies();
        return NoContent();
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var command = new ForgotPasswordCommand(request.Email);
        var result = await _passwordResetService.ForgotPasswordAsync(command, cancellationToken);
        
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Si el correo existe, recibirás instrucciones para restablecer tu contraseña." });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var command = new ResetPasswordCommand(request.Token, request.NewPassword);
        var result = await _passwordResetService.ResetPasswordAsync(command, cancellationToken);
        
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Contraseña restablecida exitosamente." });
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var command = new ChangePasswordCommand(request.CurrentPassword, request.NewPassword);
        var result = await _passwordResetService.ChangePasswordAsync(currentUser.UserId, command, cancellationToken);
        
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Contraseña cambiada exitosamente." });
    }

    [HttpGet("sessions")]
    [Authorize]
    public async Task<IActionResult> Sessions(CancellationToken cancellationToken)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var sessions = await _userSessionRepository.GetActiveSessionsByUserAsync(currentUser.UserId, cancellationToken);
        var response = sessions
            .Select(session => new SessionResponse(
                session.Id,
                session.DeviceInfo,
                session.LastSeenAt,
                session.ExpiresAt,
                session.CreatedAt))
            .ToArray();

        return Ok(response);
    }

    private static string FormatDeviceInfo(DeviceInfoDto? deviceInfo)
    {
        if (deviceInfo is null)
        {
            return "Unknown";
        }

        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(deviceInfo.Name))
        {
            parts.Add(deviceInfo.Name.Trim());
        }

        if (!string.IsNullOrWhiteSpace(deviceInfo.Id))
        {
            parts.Add($"id:{deviceInfo.Id.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(deviceInfo.UserAgent))
        {
            parts.Add(deviceInfo.UserAgent.Trim());
        }

        return parts.Count == 0 ? "Unknown" : string.Join(" | ", parts);
    }

    private AuthResponse ToAuthResponse(AuthResult result)
        => new(result.AccessToken, result.ExpiresAtUtc, result.SessionId);

    private CookieOptions CreateRefreshCookieOptions()
    {
        var sameSite = _authHttpOptions.ResolveSameSiteMode();

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = _authHttpOptions.ResolveCookieSecureFlag(Request),
            SameSite = sameSite,
            Path = "/auth",
            Expires = DateTimeOffset.UtcNow.AddDays(RefreshCookieDays),
            IsEssential = true
        };
    }

    private CookieOptions CreateSpaXsrfCookieOptions()
    {
        var sameSite = _authHttpOptions.ResolveSameSiteMode();

        return new CookieOptions
        {
            HttpOnly = false,
            Secure = _authHttpOptions.ResolveCookieSecureFlag(Request),
            SameSite = sameSite,
            Path = "/",
            IsEssential = true
        };
    }

    private CookieOptions CreateAntiforgeryCookieOptions()
    {
        var sameSite = _authHttpOptions.ResolveSameSiteMode();

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = _authHttpOptions.ResolveCookieSecureFlag(Request),
            SameSite = sameSite,
            Path = "/",
            IsEssential = true
        };
    }

    private void AppendSpaXsrfTokenCookie(string? requestToken)
    {
        if (string.IsNullOrWhiteSpace(requestToken))
        {
            return;
        }

        Response.Cookies.Append(SpaXsrfCookieName, requestToken, CreateSpaXsrfCookieOptions());
    }

    private AntiforgeryTokenSet GetAndStoreAntiforgeryTokensAsAnonymous()
    {
        var originalUser = HttpContext.User;

        try
        {
            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            return _antiforgery.GetAndStoreTokens(HttpContext);
        }
        finally
        {
            HttpContext.User = originalUser;
        }
    }

    private async Task ValidateAntiforgeryAsAnonymousAsync()
    {
        var originalUser = HttpContext.User;

        try
        {
            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            await _antiforgery.ValidateRequestAsync(HttpContext);
        }
        finally
        {
            HttpContext.User = originalUser;
        }
    }

    private bool HasAuthorizationHeader()
        => Request.Headers.TryGetValue("Authorization", out var authorizationHeader)
           && !StringValues.IsNullOrEmpty(authorizationHeader);

    private void ClearAuthCookies()
    {
        Response.Cookies.Delete(RefreshCookieName, CreateRefreshCookieOptions());
        Response.Cookies.Delete(_authHttpOptions.ResolveAntiforgeryCookieName(), CreateAntiforgeryCookieOptions());
        Response.Cookies.Delete(SpaXsrfCookieName, CreateSpaXsrfCookieOptions());
    }

    private static ProblemDetails CreateCsrfProblem()
        => new()
        {
            Title = "Invalid antiforgery token.",
            Detail = "The XSRF token is missing or invalid.",
            Status = StatusCodes.Status400BadRequest,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };

    private static ProblemDetails CreateUnauthorizedProblem()
        => new()
        {
            Title = "Unauthorized",
            Detail = "Invalid credentials or refresh token.",
            Status = StatusCodes.Status401Unauthorized,
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
        };

    private static ProblemDetails CreateRateLimitProblem(TimeSpan? retryAfter)
        => new()
        {
            Title = "Too many requests.",
            Detail = retryAfter is null
                ? "Rate limit exceeded. Try again shortly."
                : $"Rate limit exceeded. Try again in {(int)Math.Ceiling(retryAfter.Value.TotalSeconds)}s.",
            Status = StatusCodes.Status429TooManyRequests,
            Type = "https://tools.ietf.org/html/rfc6585#section-4"
        };

    private static ProblemDetails CreateAuthEndpointAuthorizationProblem()
        => new()
        {
            Title = "Authorization header is not allowed on this endpoint.",
            Detail = "Use cookie-based auth with XSRF header for /auth/login, /auth/refresh and /auth/logout.",
            Status = StatusCodes.Status400BadRequest,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };
}
