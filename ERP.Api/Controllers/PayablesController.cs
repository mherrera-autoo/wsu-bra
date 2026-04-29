using ERP.Api.Authorization;
using ERP.Api.Contracts.Payables;
using ERP.Modules.Accounting.Application.Services;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/payables")]
public sealed class PayablesController : ControllerBase
{
    private readonly PayablesService _payablesService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public PayablesController(PayablesService payablesService, ICurrentUserProvider currentUserProvider)
    {
        _payablesService = payablesService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpPost("from-purchase-order")]
    public async Task<IActionResult> CreateFromPurchaseOrder(
        CreatePayableFromPurchaseOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var schedules = request.Schedules?.Select(s => (s.DueDate, s.Amount))
            ?? Enumerable.Empty<(DateTime dueDate, decimal amount)>();

        var result = await _payablesService.CreateFromPurchaseOrderAsync(
            companyId,
            request.PurchaseOrderId,
            request.DefaultDueDate,
            schedules,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpPost("from-goods-receipt")]
    public async Task<IActionResult> CreateFromGoodsReceipt(
        CreatePayableFromGoodsReceiptRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var schedules = request.Schedules?.Select(s => (s.DueDate, s.Amount))
            ?? Enumerable.Empty<(DateTime dueDate, decimal amount)>();

        var result = await _payablesService.CreateFromGoodsReceiptAsync(
            companyId,
            request.GoodsReceiptId,
            request.DefaultDueDate,
            schedules,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var payable = await _payablesService.GetByIdAsync(id, cancellationToken);
        if (payable is null || payable.CompanyId != companyId)
        {
            return NotFound();
        }

        return Ok(new
        {
            payable.Id,
            payable.CompanyId,
            payable.SupplierId,
            payable.SourceType,
            payable.SourceId,
            payable.Status,
            TotalAmount = new { payable.TotalAmount.Amount, payable.TotalAmount.Currency },
            Schedules = payable.Schedules.Select(schedule => new
            {
                schedule.Id,
                schedule.DueDate,
                schedule.Status,
                Amount = new { schedule.Amount.Amount, schedule.Amount.Currency }
            })
        });
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
