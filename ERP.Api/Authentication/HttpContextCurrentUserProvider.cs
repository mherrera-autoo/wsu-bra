using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ERP.Modules.Identity.Application.Security;
using ERP.Shared.Application;

namespace ERP.Api.Authentication;

public sealed class HttpContextCurrentUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long? UserId => GetCurrentUser()?.UserId;

    public IReadOnlyCollection<string> Roles => GetCurrentUser()?.Roles ?? Array.Empty<string>();

    public string? Source
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                return null;
            }

            return $"api:{httpContext.Request.Path}";
        }
    }

    public CurrentUser? GetCurrentUser()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal is null || principal.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userIdValue = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var companyIdValue = principal.FindFirst(IdentityClaimTypes.CompanyId)?.Value
            ?? principal.FindFirst("companyId")?.Value;
        var companyPublicIdValue = principal.FindFirst(IdentityClaimTypes.CompanyPublicId)?.Value
            ?? principal.FindFirst("companyPublicId")?.Value;
        var organizationIdValue = principal.FindFirst(IdentityClaimTypes.OrganizationId)?.Value;
        var scope = principal.FindFirst(IdentityClaimTypes.Scope)?.Value;
        var email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value
            ?? principal.FindFirst(ClaimTypes.Email)?.Value
            ?? string.Empty;

        if (!long.TryParse(userIdValue, out var userId) || string.IsNullOrWhiteSpace(scope))
        {
            return null;
        }

        if (!long.TryParse(organizationIdValue, out var organizationId))
        {
            return null;
        }

        var companyId = 0L;
        Guid? companyPublicId = null;
        if (string.Equals(scope, "tenant", StringComparison.OrdinalIgnoreCase))
        {
            if (!long.TryParse(companyIdValue, out companyId) || companyId <= 0)
            {
                return null;
            }

            if (!Guid.TryParse(companyPublicIdValue, out var parsedCompanyPublicId))
            {
                return null;
            }

            companyPublicId = parsedCompanyPublicId;
        }
        else if (!string.Equals(scope, "platform", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
        else if (long.TryParse(companyIdValue, out var platformCompanyId))
        {
            companyId = platformCompanyId;
        }

        var roles = principal.FindAll(IdentityClaimTypes.Role)
            .Select(claim => claim.Value)
            .Concat(principal.FindAll(ClaimTypes.Role).Select(claim => claim.Value))
            .Distinct()
            .ToArray();

        return new CurrentUser(userId, organizationId, scope, companyPublicId, companyId, email, roles);
    }
}
