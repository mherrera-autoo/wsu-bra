using ERP.Api.Authorization;
using ERP.Api.Contracts.Billing;
using ERP.Modules.Billing.Application.Services;
using ERP.Shared.Application;
using ERP.Shared.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/billing")]
public sealed class BillingController : ControllerBase
{
    private readonly BillingIssuanceService _issuanceService;
    private readonly BillingStatusService _statusService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public BillingController(
        BillingIssuanceService issuanceService,
        BillingStatusService statusService,
        ICurrentUserProvider currentUserProvider)
    {
        _issuanceService = issuanceService;
        _statusService = statusService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpPost("invoices/issue")]
    public async Task<IActionResult> IssueInvoice(IssueInvoiceRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var lines = request.Lines.Select(line =>
            (line.ProductId, line.Qty, new Money(line.UnitPriceAmount, line.UnitPriceCurrency)));

        var result = await _issuanceService.IssueInvoiceAsync(
            companyId,
            request.CustomerId,
            request.ReceivableAccountId,
            request.RevenueAccountId,
            lines,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id, result.Value!.Number, result.Value!.Status });
    }

    [HttpPost("invoices/{id:long}/cancel")]
    public async Task<IActionResult> CancelInvoice(long id, CancelInvoiceRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _issuanceService.CancelInvoiceAsync(
            id,
            companyId,
            request.ReceivableAccountId,
            request.RevenueAccountId,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id, result.Value!.Status });
    }

    [HttpGet("invoices/{id:long}/status")]
    public async Task<IActionResult> GetInvoiceStatus(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _statusService.GetInvoiceStatusAsync(id, companyId, cancellationToken);
        if (!result.Success)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(new { status = result.Value!.ToString() });
    }

    [HttpPost("credit-notes/issue")]
    public async Task<IActionResult> IssueCreditNote(IssueCreditNoteRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _issuanceService.IssueCreditNoteAsync(
            companyId,
            request.InvoiceId,
            new Money(request.Amount, request.Currency),
            request.ReceivableAccountId,
            request.RevenueAccountId,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id, result.Value!.Number, result.Value!.Status });
    }

    [HttpPost("credit-notes/{id:long}/cancel")]
    public async Task<IActionResult> CancelCreditNote(long id, CancelCreditNoteRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _issuanceService.CancelCreditNoteAsync(
            id,
            companyId,
            request.ReceivableAccountId,
            request.RevenueAccountId,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id, result.Value!.Status });
    }

    [HttpGet("credit-notes/{id:long}/status")]
    public async Task<IActionResult> GetCreditNoteStatus(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _statusService.GetCreditNoteStatusAsync(id, companyId, cancellationToken);
        if (!result.Success)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(new { status = result.Value!.ToString() });
    }

    [HttpPost("debit-notes/issue")]
    public async Task<IActionResult> IssueDebitNote(IssueDebitNoteRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _issuanceService.IssueDebitNoteAsync(
            companyId,
            request.InvoiceId,
            new Money(request.Amount, request.Currency),
            request.ReceivableAccountId,
            request.RevenueAccountId,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id, result.Value!.Number, result.Value!.Status });
    }

    [HttpPost("debit-notes/{id:long}/cancel")]
    public async Task<IActionResult> CancelDebitNote(long id, CancelDebitNoteRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _issuanceService.CancelDebitNoteAsync(
            id,
            companyId,
            request.ReceivableAccountId,
            request.RevenueAccountId,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id, result.Value!.Status });
    }

    [HttpGet("debit-notes/{id:long}/status")]
    public async Task<IActionResult> GetDebitNoteStatus(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _statusService.GetDebitNoteStatusAsync(id, companyId, cancellationToken);
        if (!result.Success)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(new { status = result.Value!.ToString() });
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
