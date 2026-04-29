using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Api.Authorization;
using ERP.Api.Contracts.Identity;
using ERP.Api.Services;
using ERP.Modules.Identity.Contracts;
using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Application.Security;
using ERP.Modules.Identity.Application.Services;
using ERP.Modules.Identity.Domain;
using ERP.Modules.MasterData.Contracts;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[TenantGuard]
[Route("api/tenant")]
[EnableCors("SpaAuthCors")]
public sealed class TenantContextController : ControllerBase
{
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICompanyUserRepository _companyUserRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly ITokenService _tokenService;
    private readonly IAccessService _accessService;
    private readonly IUserRepository _userRepository;

    public TenantContextController(
        ICurrentUserProvider currentUserProvider,
        ICompanyRepository companyRepository,
        ICompanyUserRepository companyUserRepository,
        IRolePermissionRepository rolePermissionRepository,
        ITokenService tokenService,
        IAccessService accessService,
        IUserRepository userRepository)
    {
        _currentUserProvider = currentUserProvider;
        _companyRepository = companyRepository;
        _companyUserRepository = companyUserRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _tokenService = tokenService;
        _accessService = accessService;
        _userRepository = userRepository;
    }

    [HttpPost("select")]
    [Authorize]
    public async Task<IActionResult> SelectTenant(SelectTenantRequest request, CancellationToken cancellationToken)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null || !currentUser.CompanyPublicId.HasValue || currentUser.CompanyId <= 0)
        {
            return Unauthorized(new { error = "Invalid or missing current tenant context." });
        }

        if (!Guid.TryParse(request.CompanyPublicId, out var companyPublicId))
        {
            return BadRequest(new { error = "Invalid company public ID format." });
        }

        if (companyPublicId == currentUser.CompanyPublicId.Value)
        {
            return BadRequest(new { error = "Already operating in the requested company." });
        }

        var hasAccess = await _accessService.CanAccessCompanyAsync(currentUser.UserId, companyPublicId, cancellationToken);
        if (!hasAccess)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "User does not have access to the requested company." });
        }

        var targetCompany = await _companyRepository.GetByPublicIdAsync(companyPublicId, cancellationToken);
        if (targetCompany is null)
        {
            return NotFound(new { error = "Company not found." });
        }

        var organizationId = await _companyRepository.GetOrganizationIdAsync(targetCompany.Id, cancellationToken);
        if (!organizationId.HasValue)
        {
            return NotFound(new { error = "Organization not found for the company." });
        }

        var targetCompanyPublicId = targetCompany.PublicId;
        var companyUser = await _companyUserRepository.GetAsync(targetCompanyPublicId, currentUser.UserId, cancellationToken);
        if (companyUser is null || companyUser.Status != CompanyUserStatus.Active)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "User access to company is not active." });
        }

        var roles = await _userRepository.GetRoleNamesAsync(currentUser.UserId, targetCompanyPublicId, cancellationToken);
        var permissions = await _userRepository.GetPermissionCodesAsync(currentUser.UserId, targetCompanyPublicId, cancellationToken);

        var user = await _userRepository.GetByIdAsync(currentUser.UserId, cancellationToken);
        if (user is null)
        {
            return NotFound(new { error = "User not found." });
        }

        var tokenResult = _tokenService.CreateTenantToken(
            user,
            organizationId.Value,
            targetCompanyPublicId,
            targetCompany.Id,
            roles,
            permissions.ToList());

        return Ok(tokenResult);
    }
}
