using ERP.Api.Authorization;
using ERP.Api.Contracts.Pricing;
using ERP.Api.Filters;
using ERP.Modules.Pricing.Application.Services;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/pricing/policy")]
public sealed class PricingPolicyController : ControllerBase
{
    private readonly PricingPolicyService _pricingPolicyService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public PricingPolicyController(
        PricingPolicyService pricingPolicyService,
        ICurrentUserProvider currentUserProvider)
    {
        _pricingPolicyService = pricingPolicyService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet]
    [RequireCompanyPermission(PermissionKeys.Inventory.StockRead)]
    public async Task<IActionResult> GetPolicy(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var currentUser))
        {
            return Unauthorized();
        }

        var result = await _pricingPolicyService.GetAsync(currentUser.CompanyId, cancellationToken);
        if (!result.Success || result.Value is null)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [RequireCompanyPermission(PermissionKeys.Pricing.MarginWrite)]
    public async Task<IActionResult> UpsertPolicy(UpsertPricingPolicyRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var currentUser))
        {
            return Unauthorized();
        }

        if (request.CompanyId != currentUser.CompanyId)
        {
            return Forbid();
        }

        var result = await _pricingPolicyService.UpsertAsync(
            currentUser.CompanyId,
            request.MarginPercent,
            request.RoundingMode,
            request.DecimalPlaces,
            currentUser.UserId,
            cancellationToken);

        if (!result.Success || result.Value is null)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    private bool TryGetCurrentUser(out CurrentUser currentUser)
    {
        var resolved = _currentUserProvider.GetCurrentUser();
        if (resolved is null)
        {
            currentUser = null!;
            return false;
        }

        currentUser = resolved;
        return true;
    }
}
