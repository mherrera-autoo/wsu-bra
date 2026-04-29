using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ERP.Modules.Identity.Contracts;
using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Application.Security;
using ERP.Modules.Identity.Domain;
using ERP.Modules.MasterData.Contracts;
using ERP.Shared.Application;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ERP.Modules.Identity.Application.Services;

public sealed class IdentityService
{
    private static readonly TimeSpan RefreshReuseGraceWindow = TimeSpan.FromSeconds(20);

    private readonly IUserRepository _userRepository;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ICompanyUserRepository _companyUserRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IIdentityEndpointRateLimiter _identityEndpointRateLimiter;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtOptions _jwtOptions;

    private sealed record AccessTokenPayload(string Token, DateTime ExpiresAtUtc);

    public IdentityService(
        IUserRepository userRepository,
        IUserSessionRepository userSessionRepository,
        IRoleRepository roleRepository,
        ICompanyUserRepository companyUserRepository,
        ICompanyRepository companyRepository,
        IIdentityEndpointRateLimiter identityEndpointRateLimiter,
        IUnitOfWork unitOfWork,
        IOptions<JwtOptions> jwtOptions)
    {
        _userRepository = userRepository;
        _userSessionRepository = userSessionRepository;
        _roleRepository = roleRepository;
        _companyUserRepository = companyUserRepository;
        _companyRepository = companyRepository;
        _identityEndpointRateLimiter = identityEndpointRateLimiter;
        _unitOfWork = unitOfWork;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<Result<User>> CreateUserAsync(
        long companyId,
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var existingUser = await _userRepository.GetByEmailAsync(email, token);
            User user;

            if (existingUser is not null)
            {
                var company = await _companyRepository.GetByIdAsync(companyId, token);
                if (company is null)
                {
                    return Result<User>.Fail("Company not found.");
                }

                var existingCompanyUser = await _companyUserRepository.GetAsync(company.PublicId, existingUser.Id, token);
                if (existingCompanyUser is not null)
                {
                    return Result<User>.Fail("Email is already in use.");
                }

                user = existingUser;
            }
            else
            {
                var (hash, salt) = PasswordHasher.Hash(password);
                user = User.Create(email, hash, salt);

                await _userRepository.AddAsync(user, token);
                await _unitOfWork.SaveChangesAsync(token);
            }

            var companyRef = await _companyRepository.GetByIdAsync(companyId, token);
            if (companyRef is null)
            {
                return Result<User>.Fail("Company not found.");
            }

            var companyUser = await _companyUserRepository.GetAsync(companyRef.PublicId, user.Id, token);
            if (companyUser is null)
            {
                companyUser = CompanyUser.Create(companyRef.PublicId, user.Id);
                await _companyUserRepository.AddAsync(companyUser, token);
                await _unitOfWork.SaveChangesAsync(token);
            }



            await _unitOfWork.SaveChangesAsync(token);
            return Result<User>.Ok(user);
        }, cancellationToken);
    }

    public async Task<Result<BootstrapResult>> BootstrapAdminAsync(
        long companyId,
        string adminEmail,
        string adminPassword,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var role = await _roleRepository.GetByNameAsync(RoleNames.PlatformSuperAdmin, token);
            if (role is null)
            {
                role = Role.Create(RoleNames.PlatformSuperAdmin, isSystem: true);
                await _roleRepository.AddAsync(role, token);
                await _unitOfWork.SaveChangesAsync(token);
            }

            var persistedUser = await _userRepository.GetByEmailAsync(adminEmail, token);
            if (persistedUser is null)
            {
                var (hash, salt) = PasswordHasher.Hash(adminPassword);
                var user = User.Create(adminEmail, hash, salt);

                await _userRepository.AddAsync(user, token);
                await _unitOfWork.SaveChangesAsync(token);

                persistedUser = await _userRepository.GetByEmailAsync(adminEmail, token);
                if (persistedUser is null)
                {
                    return Result<BootstrapResult>.Fail("User could not be persisted during bootstrap.");
                }
            }

            var company = await _companyRepository.GetByIdAsync(companyId, token);
            if (company is null)
            {
                return Result<BootstrapResult>.Fail("Company not found.");
            }

            var companyUser = await _companyUserRepository.GetAsync(company.PublicId, persistedUser.Id, token);
            if (companyUser is null)
            {
                companyUser = CompanyUser.Create(company.PublicId, persistedUser.Id);
                await _companyUserRepository.AddAsync(companyUser, token);
                await _unitOfWork.SaveChangesAsync(token);
            }



            return Result<BootstrapResult>.Ok(new BootstrapResult(persistedUser.Id, role.Id));
        }, cancellationToken);
    }

    public async Task<Result> DeactivateUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result.Fail("User not found.");
        }

        user.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result<AuthResult>> LoginAsync(
        string email,
        string password,
        string deviceInfo,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return Result<AuthResult>.Fail("Invalid credentials.");
        }

        if (!PasswordHasher.Verify(password, user.PasswordHash, user.PasswordSalt))
        {
            return Result<AuthResult>.Fail("Invalid credentials.");
        }

        var accessTokenResult = await GenerateTenantAccessTokenAsync(user, cancellationToken);
        if (!accessTokenResult.Success || accessTokenResult.Value is null)
        {
            return Result<AuthResult>.Fail(accessTokenResult.Error ?? "Access token generation failed.");
        }

        var refreshToken = GenerateRefreshToken();
        var refreshTokenHash = HashRefreshToken(refreshToken);
        var refreshExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);

        var session = UserSession.Create(user.Id, deviceInfo, refreshTokenHash, refreshExpiresAt);
        await _userSessionRepository.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AuthResult>.Ok(new AuthResult(
            accessTokenResult.Value.Token,
            accessTokenResult.Value.ExpiresAtUtc,
            session.Id,
            refreshToken));
    }

    public async Task<RefreshTokenResult> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return new RefreshTokenResult(RefreshTokenStatus.Invalid, Error: "Invalid refresh token.");
        }

        var refreshTokenHash = HashRefreshToken(refreshToken);
        var refreshResult = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var session = await _userSessionRepository.GetByRefreshTokenHashForUpdateAsync(refreshTokenHash, token);
            if (session is null)
            {
                return Result<RefreshTokenResult>.Ok(new RefreshTokenResult(RefreshTokenStatus.Invalid, Error: "Invalid refresh token."));
            }

            if (!_identityEndpointRateLimiter.TryConsumeRefresh(session.SessionId, out var refreshRetryAfter))
            {
                return Result<RefreshTokenResult>.Ok(new RefreshTokenResult(
                    RefreshTokenStatus.RateLimited,
                    Error: "Too many refresh requests.",
                    RetryAfter: refreshRetryAfter));
            }

            var now = DateTime.UtcNow;
            if (session.ExpiresAt <= now)
            {
                return Result<RefreshTokenResult>.Ok(new RefreshTokenResult(RefreshTokenStatus.Invalid, Error: "Invalid refresh token."));
            }

            if (session.ReplacedAtUtc is not null)
            {
                if (now - session.ReplacedAtUtc.Value <= RefreshReuseGraceWindow)
                {
                    var graceTokenResult = await GenerateTenantAccessTokenAsync(session.UserId, token);
                    if (!graceTokenResult.Success || graceTokenResult.Value is null)
                    {
                        return Result<RefreshTokenResult>.Ok(new RefreshTokenResult(RefreshTokenStatus.Invalid, Error: graceTokenResult.Error ?? "Invalid refresh token."));
                    }

                    return Result<RefreshTokenResult>.Ok(new RefreshTokenResult(
                        RefreshTokenStatus.ReusedWithinGrace,
                        new AuthResult(graceTokenResult.Value.Token, graceTokenResult.Value.ExpiresAtUtc, session.Id)));
                }

                await _userSessionRepository.RevokeBySessionIdAsync(session.SessionId, token);

                return Result<RefreshTokenResult>.Ok(new RefreshTokenResult(RefreshTokenStatus.Compromised, Error: "Refresh token reuse detected."));
            }

            if (session.RevokedAt is not null)
            {
                return Result<RefreshTokenResult>.Ok(new RefreshTokenResult(RefreshTokenStatus.Invalid, Error: "Invalid refresh token."));
            }

            var accessTokenResult = await GenerateTenantAccessTokenAsync(session.UserId, token);
            if (!accessTokenResult.Success || accessTokenResult.Value is null)
            {
                return Result<RefreshTokenResult>.Ok(new RefreshTokenResult(RefreshTokenStatus.Invalid, Error: accessTokenResult.Error ?? "Invalid refresh token."));
            }

            var nextRefreshToken = GenerateRefreshToken();
            var nextRefreshTokenHash = HashRefreshToken(nextRefreshToken);
            var refreshExpiresAt = now.AddDays(_jwtOptions.RefreshTokenDays);

            session.MarkSeen(now);
            session.MarkReplaced(nextRefreshTokenHash, now);

            var nextSession = UserSession.Create(session.UserId, session.DeviceInfo, nextRefreshTokenHash, refreshExpiresAt, session.SessionId);
            await _userSessionRepository.AddAsync(nextSession, token);

            return Result<RefreshTokenResult>.Ok(new RefreshTokenResult(
                RefreshTokenStatus.Success,
                new AuthResult(accessTokenResult.Value.Token, accessTokenResult.Value.ExpiresAtUtc, nextSession.Id, nextRefreshToken)));
        }, cancellationToken);

        if (!refreshResult.Success || refreshResult.Value is null)
        {
            return new RefreshTokenResult(RefreshTokenStatus.Invalid, Error: "Invalid refresh token.");
        }

        return refreshResult.Value;
    }

    public async Task<Result> RevokeSessionAsync(
        string? refreshToken,
        long? sessionId,
        CancellationToken cancellationToken = default)
    {
        var revoked = false;

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var refreshTokenHash = HashRefreshToken(refreshToken);
            revoked |= await _userSessionRepository.RevokeByRefreshTokenHashAsync(refreshTokenHash, cancellationToken);
        }

        if (sessionId is not null)
        {
            revoked |= await _userSessionRepository.RevokeByIdAsync(sessionId.Value, cancellationToken);
        }

        if (revoked)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Ok();
    }

    private async Task<Result<AccessTokenPayload>> GenerateTenantAccessTokenAsync(long userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return Result<AccessTokenPayload>.Fail("Invalid refresh token.");
        }

        return await GenerateTenantAccessTokenAsync(user, cancellationToken);
    }

    private async Task<Result<AccessTokenPayload>> GenerateTenantAccessTokenAsync(User user, CancellationToken cancellationToken)
    {
        var companyPublicId = await _companyUserRepository.GetActiveCompanyPublicIdAsync(user.Id, cancellationToken);
        if (!companyPublicId.HasValue)
        {
            return Result<AccessTokenPayload>.Fail("User is not assigned to a company.");
        }

        var companyId = await _companyRepository.GetIdByPublicIdAsync(companyPublicId.Value, cancellationToken);
        if (!companyId.HasValue)
        {
            return Result<AccessTokenPayload>.Fail("Company not found.");
        }

        var roles = await _userRepository.GetRoleNamesAsync(user.Id, companyPublicId.Value, cancellationToken);
        var permissions = await _userRepository.GetPermissionCodesAsync(user.Id, companyPublicId.Value, cancellationToken);
        var organizationId = await _companyRepository.GetOrganizationIdAsync(companyId.Value, cancellationToken);
        if (!organizationId.HasValue)
        {
            return Result<AccessTokenPayload>.Fail("Organization not found.");
        }

        return GenerateAccessToken(user, organizationId.Value, companyPublicId.Value, companyId.Value, roles, permissions);
    }

    private Result<AccessTokenPayload> GenerateAccessToken(
        User user,
        long organizationId,
        Guid companyPublicId,
        long companyId,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions)
    {
        if (string.IsNullOrWhiteSpace(_jwtOptions.SigningKey))
        {
            return Result<AccessTokenPayload>.Fail("JWT signing key is missing.");
        }

        var keyBytes = Encoding.UTF8.GetBytes(_jwtOptions.SigningKey);
        if (keyBytes.Length < 32)
        {
            return Result<AccessTokenPayload>.Fail("JWT signing key must be at least 256 bits (32 bytes).");
        }

        var key = new SymmetricSecurityKey(keyBytes);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(IdentityClaimTypes.Scope, "tenant"),
            new(IdentityClaimTypes.OrganizationId, organizationId.ToString()),
            new(IdentityClaimTypes.CompanyPublicId, companyPublicId.ToString()),
            new(IdentityClaimTypes.CompanyId, companyId.ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(roles.Select(role => new Claim(IdentityClaimTypes.Role, role)));

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return Result<AccessTokenPayload>.Ok(new AccessTokenPayload(new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc));
    }

    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private string HashRefreshToken(string refreshToken)
    {
        var pepper = _jwtOptions.RefreshTokenPepper ?? string.Empty;
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{refreshToken}:{pepper}"));
        return Convert.ToBase64String(bytes);
    }
}
