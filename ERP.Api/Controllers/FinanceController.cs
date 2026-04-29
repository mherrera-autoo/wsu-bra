using ERP.Api.Authorization;
using ERP.Api.Contracts.Finance;
using ERP.Modules.Finance.Application.Services;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/finance")]
public sealed class FinanceController : ControllerBase
{
    private readonly FinanceService _financeService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public FinanceController(FinanceService financeService, ICurrentUserProvider currentUserProvider)
    {
        _financeService = financeService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpPost("receivables/from-sales")]
    public async Task<IActionResult> CreateReceivableFromSales(
        CreateAccountsReceivableRequest request,
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

        var result = await _financeService.GenerateAccountsReceivableFromSalesAsync(
            companyId,
            request.SalesDocumentId,
            request.DueDate,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id, result.Value!.Status });
    }

    [HttpPost("payables/from-purchase-orders")]
    public async Task<IActionResult> CreatePayableFromPurchaseOrder(
        CreateAccountsPayableRequest request,
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

        var result = await _financeService.GenerateAccountsPayableFromPurchaseOrderAsync(
            companyId,
            request.PurchaseOrderId,
            request.DueDate,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id, result.Value!.Status });
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
