using System;
using ERP.Api.Authorization;
using ERP.Api.Contracts.Identity;
using ERP.Api.Services;
using ERP.Modules.Identity.Contracts;
using ERP.Modules.Identity.Application.Services;
using ERP.Modules.MasterData.Contracts;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[TenantGuard]
[Route("api/identity")]
public sealed class IdentityController : ControllerBase
{
    private readonly UserService _userService;
    private readonly IdentityService _identityService;
    private readonly IConfiguration _configuration;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly TaxEntityService _taxEntityService;
    private readonly CompanyAccessService _companyAccessService;
    private readonly BootstrapCompanyService _bootstrapCompanyService;
    private readonly IWorkspaceResolver _workspaceResolver;
    private readonly UserPermissionsService _userPermissionsService;
    private readonly ICompanyRepository _companyRepository;
    public IdentityController(
        UserService userService,
        IdentityService identityService,
        IConfiguration configuration,
        ICurrentUserProvider currentUserProvider,
        TaxEntityService taxEntityService,
        CompanyAccessService companyAccessService,
        BootstrapCompanyService bootstrapCompanyService,
        IWorkspaceResolver workspaceResolver,
        UserPermissionsService userPermissionsService,
        ICompanyRepository companyRepository)
    {
        _userService = userService;
        _identityService = identityService;
        _configuration = configuration;
        _currentUserProvider = currentUserProvider;
        _taxEntityService = taxEntityService;
        _companyAccessService = companyAccessService;
        _bootstrapCompanyService = bootstrapCompanyService;
        _workspaceResolver = workspaceResolver;
        _userPermissionsService = userPermissionsService;
        _companyRepository = companyRepository;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null)
        {
            return Unauthorized();
        }

        if (string.Equals(currentUser.Scope, "tenant", StringComparison.OrdinalIgnoreCase)
            && currentUser.CompanyId > 0)
        {
            var organizationId = await _companyRepository.GetOrganizationIdAsync(
                currentUser.CompanyId,
                cancellationToken);
            if (organizationId.HasValue && organizationId.Value != currentUser.OrganizationId)
            {
                currentUser = currentUser with { OrganizationId = organizationId.Value };
            }
        }

        return Ok(currentUser);
    }

    [HttpGet("me/permissions")]
    [Authorize]
    public async Task<IActionResult> GetMyPermissions(CancellationToken cancellationToken)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var permissions = await _userPermissionsService.GetUserPermissionsAsync(cancellationToken);
        return Ok(permissions);
    }

    [HttpGet("companies")]
    [Authorize]
    public async Task<IActionResult> ListCompanies(
        [FromQuery] CompanyListScope scope = CompanyListScope.OperableOnly,
        [FromQuery] long? workspaceOrganizationId = null,
        CancellationToken cancellationToken = default)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null)
        {
            return Unauthorized();
        }

        long resolvedWorkspaceOrganizationId;
        if (workspaceOrganizationId.HasValue)
        {
            var isMember = await _workspaceResolver.IsOrganizationMemberAsync(
                currentUser.UserId,
                workspaceOrganizationId.Value,
                cancellationToken);
            if (!isMember)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { error = "User is not a member of the requested workspace organization." });
            }

            resolvedWorkspaceOrganizationId = workspaceOrganizationId.Value;
        }
        else
        {
            try
            {
                resolvedWorkspaceOrganizationId = await _workspaceResolver.ResolveWorkspaceOrganizationIdAsync(
                    currentUser.UserId,
                    cancellationToken);
            }
            catch (WorkspaceResolutionException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
            }
        }

        var companies = await _companyAccessService.ListForUserAsync(
            currentUser.UserId,
            resolvedWorkspaceOrganizationId,
            scope,
            cancellationToken);
        return Ok(companies);
    }

    [HttpGet("tax-entities")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<TaxEntityAccessSummary>>> ListTaxEntities(CancellationToken cancellationToken)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null)
        {
            return Unauthorized();
        }

        long resolvedWorkspaceOrganizationId;
        try
        {
            resolvedWorkspaceOrganizationId = await _workspaceResolver.ResolveWorkspaceOrganizationIdAsync(
                currentUser.UserId,
                cancellationToken);
        }
        catch (WorkspaceResolutionException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }

        var companies = await _companyAccessService.ListForUserAsync(
            currentUser.UserId,
            resolvedWorkspaceOrganizationId,
            CompanyListScope.Available,
            cancellationToken);

        var summaries = await _taxEntityService.ListAccessibleAsync(companies, cancellationToken);
        return Ok(summaries);
    }

    [HttpGet("tax-entities/current")]
    [Authorize]
    public async Task<ActionResult<TaxEntitySummary>> GetCurrentTaxEntity(CancellationToken cancellationToken)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null || currentUser.CompanyId <= 0)
        {
            return Unauthorized();
        }

        var taxEntity = await _taxEntityService.GetCompanyTaxEntityAsync(currentUser.CompanyId, cancellationToken);
        if (taxEntity is null)
        {
            return NotFound();
        }

        return Ok(new TaxEntitySummary(taxEntity.TaxEntityId, taxEntity.TaxId, taxEntity.DisplayName));
    }

    [HttpPost("users")]
    [Authorize]
    public async Task<IActionResult> CreateUser(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null || !currentUser.CompanyPublicId.HasValue)
        {
            return Unauthorized();
        }
        var result = await _userService.CreateUserAsync(request.Email, request.Password, cancellationToken);
        if (!result.Success)
        {
            if (string.Equals(result.Error, "Company not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPut("users/{id:long}")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(long id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null || !currentUser.CompanyPublicId.HasValue)
        {
            return Unauthorized();
        }

        if (request.CompanyPublicId != currentUser.CompanyPublicId.Value)
        {
            return Forbid();
        }

        var result = await _userService.UpdateUserAsync(
            request.CompanyPublicId,
            currentUser.CompanyId,
            id,
            request.Email,
            request.Password,
            request.IsActive,
            cancellationToken);

        if (!result.Success)
        {
            if (string.Equals(result.Error, "User not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpDelete("users/{id:long}")]
    [Authorize]
    public async Task<IActionResult> DeactivateUser(long id, CancellationToken cancellationToken)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null || !currentUser.CompanyPublicId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _userService.DeactivateUserAsync(currentUser.CompanyPublicId.Value, id, cancellationToken);
        if (!result.Success)
        {
            if (string.Equals(result.Error, "User not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "deactivated" });
    }

    [HttpPost("users/{userPublicId:guid}/companies")]
    [Authorize]
    public async Task<IActionResult> AssociateUserToCompany(Guid userPublicId, AssociateUserToCompanyRequest request, CancellationToken cancellationToken)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var result = await _userService.AssociateUserToCompanyAsync(userPublicId, request.CompanyPublicId, cancellationToken);
        if (!result.Success)
        {
            if (string.Equals(result.Error, "User not found.", StringComparison.OrdinalIgnoreCase)
                || string.Equals(result.Error, "Company not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPost("bootstrap")]
    [AllowAnonymous]
    public async Task<IActionResult> Bootstrap(BootstrapRequest request, CancellationToken cancellationToken)
        => await BootstrapFromSectionAsync("Bootstrap", request, cancellationToken);

    [HttpPost("rfidbootstrap")]
    [AllowAnonymous]
    public async Task<IActionResult> RfidBootstrap(BootstrapRequest request, CancellationToken cancellationToken)
        => await BootstrapFromSectionAsync("RfidBootstrap", request, cancellationToken);

    private async Task<IActionResult> BootstrapFromSectionAsync(
        string sectionName,
        BootstrapRequest request,
        CancellationToken cancellationToken)
    {
        var bootstrapSection = _configuration.GetSection(sectionName);
        var adminEmail = bootstrapSection["AdminEmail"];
        var adminPassword = bootstrapSection["AdminPassword"];
        var bootstrapToken = bootstrapSection["BootstrapToken"];
        var organizationDefinition = bootstrapSection.GetSection("Organization").Get<BootstrapOrganizationDefinition>()
            ?? new BootstrapOrganizationDefinition();
        var companySection = bootstrapSection.GetSection("Company");
        var companies = companySection.Get<List<BootstrapCompanyDefinition>>() ?? new List<BootstrapCompanyDefinition>();

        if (string.IsNullOrWhiteSpace(adminEmail)
            || string.IsNullOrWhiteSpace(adminPassword)
            || companies.Count == 0
            || organizationDefinition.AccountType is null
            || string.IsNullOrWhiteSpace(bootstrapToken))
        {
            return BadRequest(new { error = "Bootstrap configuration is missing." });
        }

        if (!string.Equals(request.Token, bootstrapToken, StringComparison.Ordinal))
        {
            return Unauthorized(new { error = "Invalid bootstrap token." });
        }

        var companyResult = await _bootstrapCompanyService.EnsureCompaniesAsync(
            organizationDefinition,
            companies,
            cancellationToken);
        if (!companyResult.Success || companyResult.Value is null || companyResult.Value.CompanyIds.Count == 0)
        {
            return BadRequest(new { error = companyResult.Error ?? "Bootstrap company could not be created." });
        }

        var companyValue = companyResult.Value;
        BootstrapResult? bootstrapResult = null;
        foreach (var companyId in companyValue.CompanyIds)
        {
            var result = await _identityService.BootstrapAdminAsync(
                companyId,
                adminEmail,
                adminPassword,
                cancellationToken);
            if (!result.Success)
            {
                return Conflict(new BootstrapResponse(false, false, null, result.Error ?? "Bootstrap failed."));
            }

            bootstrapResult ??= result.Value;
        }

        if (bootstrapResult is not null)
        {
            await _bootstrapCompanyService.EnsureWorkspaceOwnerAsync(
                companyValue.WorkspaceOrganizationId,
                bootstrapResult.UserId,
                cancellationToken);
            await _bootstrapCompanyService.EnsurePlatformAdminAsync(
                bootstrapResult.UserId,
                cancellationToken);

            foreach (var companyId in companyValue.CompanyIds)
            {
                await _bootstrapCompanyService.EnsureCompanyOwnerAsync(
                    companyId,
                    bootstrapResult.UserId,
                    cancellationToken);

                if (string.Equals(sectionName, "RfidBootstrap", StringComparison.Ordinal))
                {
                    await _bootstrapCompanyService.EnsureRfidManagerAsync(
                        companyId,
                        bootstrapResult.UserId,
                        cancellationToken);
                }
            }
        }

        return Ok(new BootstrapResponse(true, false, bootstrapResult?.UserId, "Bootstrap completed."));
    }
}
