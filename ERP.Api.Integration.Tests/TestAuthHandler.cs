using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using ERP.Modules.Identity.Application.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ERP.Api.Integration.Tests;

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public new const string Scheme = "Test";
    public const string UserIdHeader = "x-test-user-id";
    public const string CompanyIdHeader = "x-test-company-id";
    public const string OrganizationIdHeader = "x-test-organization-id";
    public const string CompanyPublicIdHeader = "x-test-company-public-id";
    public const string ScopeHeader = "x-test-scope";
    public const string RolesHeader = "x-test-roles";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Request.Headers[UserIdHeader].ToString();
        var companyId = Request.Headers[CompanyIdHeader].ToString();
        var organizationId = Request.Headers[OrganizationIdHeader].ToString();
        var scope = Request.Headers[ScopeHeader].ToString();
        var companyPublicId = Request.Headers[CompanyPublicIdHeader].ToString();
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(scope) || string.IsNullOrWhiteSpace(organizationId))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing test identity headers."));
        }

        if (string.Equals(scope, "tenant", StringComparison.OrdinalIgnoreCase)
            && (string.IsNullOrWhiteSpace(companyId) || string.IsNullOrWhiteSpace(companyPublicId)))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing test tenant company header."));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(IdentityClaimTypes.Scope, scope),
            new(IdentityClaimTypes.OrganizationId, organizationId)
        };

        if (!string.IsNullOrWhiteSpace(companyId))
        {
            claims.Add(new Claim(IdentityClaimTypes.CompanyId, companyId));
        }

        if (!string.IsNullOrWhiteSpace(companyPublicId))
        {
            claims.Add(new Claim(IdentityClaimTypes.CompanyPublicId, companyPublicId));
        }

        var roles = Request.Headers[RolesHeader].ToString()
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var role in roles)
        {
            claims.Add(new Claim(IdentityClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
