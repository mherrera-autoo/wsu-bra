using ERP.Api.Authorization;
using ERP.Api.Contracts.Integrations;
using ERP.Modules.Integrations.Contracts;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/integrations/automation")]
public sealed class AutomationProvidersController : ControllerBase
{
    private readonly IAutomationProviderRegistry _registry;
    private readonly ITenantContext _tenantContext;

    public AutomationProvidersController(IAutomationProviderRegistry registry, ITenantContext tenantContext)
    {
        _registry = registry;
        _tenantContext = tenantContext;
    }

    [HttpPost("providers")]
    [RequireCompanyPermission(PermissionKeys.Accounting.AutomationRulesManage)]
    public async Task<IActionResult> RegisterProvider(
        AutomationProviderRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var registration = new AutomationProviderRegistration(
            companyId,
            request.ProviderKey,
            request.DisplayName,
            request.ProviderType,
            request.Endpoint,
            request.AuthenticationScheme);

        await _registry.RegisterAsync(registration, cancellationToken);
        return Ok(new { status = "registered" });
    }

    [HttpGet("providers")]
    [RequireCompanyPermission(PermissionKeys.Accounting.AutomationRulesManage)]
    public async Task<ActionResult<IReadOnlyList<AutomationProviderRegistrationResponse>>> ListProviders(
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var registrations = await _registry.ListAsync(companyId, cancellationToken);
        var response = registrations.Select(registration => new AutomationProviderRegistrationResponse(
            registration.ProviderKey,
            registration.DisplayName,
            registration.ProviderType,
            registration.Endpoint,
            registration.AuthenticationScheme));

        return Ok(response);
    }

    [HttpPost("dispatch")]
    [RequireCompanyPermission(PermissionKeys.Accounting.AutomationRulesManage)]
    public async Task<ActionResult> DispatchAutomation(
        AutomationDispatchRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _registry.DispatchAsync(
            companyId,
            request.ProviderKey,
            new AutomationRequest(companyId, request.ProviderKey, request.Operation, request.Payload),
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Message });
        }

        return Ok(new { status = "dispatched", result.ExternalReference, result.Message });
    }

    private bool TryGetCompanyId(out long companyId)
    {
        companyId = _tenantContext.CompanyId ?? 0;
        return companyId > 0;
    }
}
