using ERP.Api.Authorization;
using ERP.Api.Contracts.Subscriptions;
using ERP.Modules.Subscriptions.Application.Services;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/subscriptions")]
public sealed class SubscriptionsController : ControllerBase
{
    private readonly SubscriptionService _subscriptionService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public SubscriptionsController(SubscriptionService subscriptionService, ICurrentUserProvider currentUserProvider)
    {
        _subscriptionService = subscriptionService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpPost("onboard")]
    public async Task<IActionResult> OnboardCompany(OnboardCompanySubscriptionRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var userId = _currentUserProvider.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _subscriptionService.OnboardCompanyAsync(
            companyId,
            request.PlanId,
            request.SeatLimit,
            request.TrialDays,
            userId.Value,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(ToResponse(result.Value!));
    }

    [HttpPost("upgrade")]
    public async Task<IActionResult> UpgradePlan(ChangeSubscriptionPlanRequest request, CancellationToken cancellationToken)
    {
        return await ChangePlanAsync(request, cancellationToken);
    }

    [HttpPost("downgrade")]
    public async Task<IActionResult> DowngradePlan(ChangeSubscriptionPlanRequest request, CancellationToken cancellationToken)
    {
        return await ChangePlanAsync(request, cancellationToken);
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> Cancel(CancelSubscriptionRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var userId = _currentUserProvider.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _subscriptionService.CancelAsync(
            companyId,
            userId.Value,
            request.Reason,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(ToResponse(result.Value!));
    }

    private async Task<IActionResult> ChangePlanAsync(ChangeSubscriptionPlanRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var userId = _currentUserProvider.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _subscriptionService.ChangePlanAsync(
            companyId,
            request.PlanId,
            userId.Value,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(ToResponse(result.Value!));
    }

    private static SubscriptionResponse ToResponse(ERP.Modules.Subscriptions.Application.Models.SubscriptionSummary summary)
        => new(
            summary.Id,
            summary.CompanyId,
            summary.PlanId,
            summary.Status,
            summary.StartedAt,
            summary.CancelledAt);

    private bool TryGetCompanyId(out long companyId)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null)
        {
            companyId = default;
            return false;
        }

        companyId = currentUser.CompanyId;
        return true;
    }
}
