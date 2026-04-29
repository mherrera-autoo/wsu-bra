using System;
using ERP.Modules.Identity.Contracts;
using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Application.Security;
using ERP.Modules.MasterData.Contracts;
using ERP.Shared.Application;

namespace ERP.Modules.Identity.Application.Services;

public sealed class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICompanyRepository _companyRepository;
    private readonly ITokenService _tokenService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ICompanyRepository companyRepository,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _companyRepository = companyRepository;
        _tokenService = tokenService;
    }

    public async Task<Result<LoginResult>> LoginAsync(long companyId, Guid companyPublicId, string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(companyPublicId, email, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return Result<LoginResult>.Fail("Invalid credentials.");
        }

        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
        {
            return Result<LoginResult>.Fail("Invalid credentials.");
        }

        var roles = await _userRepository.GetRoleNamesAsync(user.Id, companyPublicId, cancellationToken);
        var permissions = await _userRepository.GetPermissionCodesAsync(user.Id, companyPublicId, cancellationToken);

        var organizationId = await _companyRepository.GetOrganizationIdAsync(companyId, cancellationToken);
        if (!organizationId.HasValue)
        {
            return Result<LoginResult>.Fail("Organization not found.");
        }

        var token = _tokenService.CreateTenantToken(user, organizationId.Value, companyPublicId, companyId, roles, permissions);
        return Result<LoginResult>.Ok(token);
    }
}
