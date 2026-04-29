using ERP.Api.Authorization;
using ERP.Api.Contracts.Receivables;
using ERP.Modules.Accounting.Application.Services;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/receivables")]
public sealed class ReceivablesController : ControllerBase
{
    private readonly ReceivablesService _receivablesService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public ReceivablesController(ReceivablesService receivablesService, ICurrentUserProvider currentUserProvider)
    {
        _receivablesService = receivablesService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ReceivableDto>>> GetReceivables(
        [FromQuery] DateTime? asOf,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var asOfDate = (asOf ?? DateTime.UtcNow).Date;
        var receivables = await _receivablesService.GetReceivablesAsync(companyId, asOfDate, cancellationToken);

        var response = receivables
            .Select(receivable => new ReceivableDto(
                receivable.Id,
                receivable.CustomerId,
                receivable.Origin.ToString(),
                receivable.Status.ToString(),
                receivable.Currency,
                receivable.TotalAmount,
                receivable.OutstandingAmount,
                receivable.IssueDate,
                receivable.Schedules
                    .Select(schedule => new ReceivableScheduleDto(
                        schedule.DueDate,
                        schedule.Amount,
                        schedule.Status.ToString()))
                    .ToList()))
            .ToList();

        return Ok(response);
    }

    [HttpGet("aging")]
    public async Task<ActionResult<ReceivableAgingResponse>> GetAging(
        [FromQuery] DateTime? asOf,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var asOfDate = (asOf ?? DateTime.UtcNow).Date;
        var aging = await _receivablesService.GetAgingAsync(companyId, asOfDate, cancellationToken);

        var response = new ReceivableAgingResponse(
            aging.CompanyId,
            aging.AsOf,
            aging.Buckets
                .Select(bucket => new ReceivableAgingBucketDto(bucket.Bucket, bucket.Amount))
                .ToList(),
            aging.TotalOutstanding);

        return Ok(response);
    }

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
