using System;
using ERP.Api.Authorization;
using ERP.Api.Contracts.Onboarding;
using ERP.Modules.Identity.Application.Services;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/onboarding")]
public sealed class OnboardingController : ControllerBase
{
    private readonly OnboardingService _onboardingService;    
    private readonly ICurrentUserProvider _currentUserProvider;

    public OnboardingController(
        OnboardingService onboardingService,
        ICurrentUserProvider currentUserProvider)
    {
        _onboardingService = onboardingService;        
        _currentUserProvider = currentUserProvider;
    }    

    [HttpPost("multi-company")]
    [RequireOrganizationPermission("Workspace.Manage")]
    public async Task<IActionResult> OnboardMultiCompany(OnboardMultiCompanyRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserProvider.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _onboardingService.OnboardMultiCompanyAsync(
            userId.Value,
            request.DisplayName,
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

    [HttpPost("individual")]
    [RequireOrganizationPermission("Workspace.Companies.Create")]
    public async Task<IActionResult> OnboardIndividual(OnboardIndividualRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserProvider.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _onboardingService.OnboardIndividualAsync(
            userId.Value,
            request.TaxId,
            request.DisplayName,
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

    [HttpPost("holding")]
    [RequireOrganizationPermission("Workspace.Manage")]
    public async Task<IActionResult> OnboardHolding(OnboardHoldingRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserProvider.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _onboardingService.OnboardHoldingAsync(
            userId.Value,
            request.DisplayName,
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

    [HttpPost("multi-company/{organizationId:long}/clients")]
    [RequireOrganizationPermission("Workspace.Companies.Create")]
    public async Task<IActionResult> AddClientCompany(long organizationId, AddClientCompanyRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserProvider.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _onboardingService.AddClientCompanyAsync(
            organizationId,
            userId.Value,
            request.TaxId,
            request.DisplayName,
            cancellationToken);

        if (!result.Success)
        {
            if (string.Equals(result.Error, "Organization not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            if (string.Equals(result.Error, "User is not a member of the organization.", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPost("select-company")]
    public async Task<IActionResult> SelectCompany(SelectCompanyRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserProvider.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _onboardingService.SelectCompanyAsync(userId.Value, request.CompanyPublicId, cancellationToken);
        if (!result.Success)
        {
            if (string.Equals(result.Error, "Organization not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            if (string.Equals(result.Error, "User does not have access to the company.", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}
