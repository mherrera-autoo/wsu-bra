using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ERP.Modules.Identity.Contracts;
using ERP.Modules.Identity.Application.Security;
using ERP.Modules.Identity.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ERP.Api.Authentication;

public sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public LoginResult CreateTenantToken(
        User user,
        long organizationId,
        Guid companyPublicId,
        long companyId,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(IdentityClaimTypes.Scope, "tenant"),
            new(IdentityClaimTypes.OrganizationId, organizationId.ToString()),
            new(IdentityClaimTypes.CompanyPublicId, companyPublicId.ToString()),
            new(IdentityClaimTypes.CompanyId, companyId.ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(IdentityClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var accessToken = _tokenHandler.WriteToken(token);

        var userInfo = new AuthUserInfo(user.Id, companyPublicId, companyId, user.Email, roles, permissions);

        return new LoginResult(accessToken, expires, userInfo);
    }

    public LoginResult CreatePlatformToken(User user, long organizationId, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(IdentityClaimTypes.Scope, "platform"),
            new(IdentityClaimTypes.OrganizationId, organizationId.ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(IdentityClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var accessToken = _tokenHandler.WriteToken(token);

        var userInfo = new AuthUserInfo(user.Id, null, 0, user.Email, roles, permissions);

        return new LoginResult(accessToken, expires, userInfo);
    }
}
