using ERP.Api.Authorization;
using ERP.Api.Contracts.Accounting;
using ERP.Modules.Accounting.Application.Services;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/accounting")]
public sealed class AccountingController : ControllerBase
{
    private readonly AccountingService _accountingService;
    private readonly AccountingBootstrapService _bootstrapService;
    private readonly AccountingPeriodService _periodService;
    private readonly ITenantContext _tenantContext;

    public AccountingController(
        AccountingService accountingService,
        AccountingBootstrapService bootstrapService,
        AccountingPeriodService periodService,
        ITenantContext tenantContext)
    {
        _accountingService = accountingService;
        _bootstrapService = bootstrapService;
        _periodService = periodService;
        _tenantContext = tenantContext;
    }

    [HttpPost("bootstrap")]
    [RequireCompanyPermission(PermissionKeys.Accounting.ChartOfAccountsWrite)]
    public async Task<IActionResult> Bootstrap(BootstrapAccountingRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _bootstrapService.EnsureSeededAsync(
            companyId,
            request.BaseCurrencyCode,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "seeded" });
    }

    [HttpPost("accounts")]
    [RequireCompanyPermission(PermissionKeys.Accounting.ChartOfAccountsWrite)]
    public async Task<IActionResult> CreateAccount(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _accountingService.CreateAccountAsync(
            companyId,
            request.Code,
            request.Name,
            request.AccountType,
            request.ParentId,
            request.IsActive,
            request.SortOrder,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpGet("accounts")]
    [RequireCompanyPermission(PermissionKeys.Accounting.ChartOfAccountsRead)]
    public async Task<ActionResult<IReadOnlyList<AccountSummary>>> ListAccounts(
        [FromQuery] string? search,
        [FromQuery] bool includeInactive = false,
        [FromQuery] bool includeSystem = true,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var accounts = await _accountingService.ListAccountsAsync(
            companyId,
            search,
            includeInactive,
            includeSystem,
            cancellationToken);
        var response = accounts.Select(account => new AccountSummary(
            account.Id,
            account.Code,
            account.Name,
            account.AccountType,
            account.ParentId,
            account.IsActive,
            account.IsSystemRequired,
            account.IsLocked,
            account.SortOrder));
        return Ok(response);
    }

    [HttpGet("accounts/{id:long}")]
    [RequireCompanyPermission(PermissionKeys.Accounting.ChartOfAccountsRead)]
    public async Task<ActionResult<AccountSummary>> GetAccount(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var account = await _accountingService.GetAccountAsync(companyId, id, cancellationToken);
        if (account is null)
        {
            return NotFound();
        }

        return Ok(new AccountSummary(
            account.Id,
            account.Code,
            account.Name,
            account.AccountType,
            account.ParentId,
            account.IsActive,
            account.IsSystemRequired,
            account.IsLocked,
            account.SortOrder));
    }

    [HttpPut("accounts/{id:long}")]
    [RequireCompanyPermission(PermissionKeys.Accounting.ChartOfAccountsWrite)]
    public async Task<IActionResult> UpdateAccount(long id, UpdateAccountRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _accountingService.UpdateAccountAsync(
            companyId,
            id,
            request.Code,
            request.Name,
            request.AccountType,
            request.ParentId,
            request.IsActive,
            request.SortOrder,
            cancellationToken);

        if (!result.Success)
        {
            if (string.Equals(result.Error, "Account not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpDelete("accounts/{id:long}")]
    [RequireCompanyPermission(PermissionKeys.Accounting.ChartOfAccountsWrite)]
    public async Task<IActionResult> DeleteAccount(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _accountingService.DeleteAccountAsync(companyId, id, cancellationToken);
        if (!result.Success)
        {
            if (string.Equals(result.Error, "Account not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "deleted" });
    }

    [HttpPost("periods/close")]
    [RequireCompanyPermission(PermissionKeys.Accounting.PeriodClose)]
    public async Task<IActionResult> ClosePeriod(
        ERP.Api.Contracts.Accounting.AccountingPeriodCloseRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var closedBy = _tenantContext.UserId;
        var result = await _periodService.ClosePeriodAsync(
            companyId,
            request.Year,
            request.Month,
            closedBy,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "closed" });
    }

    private bool TryGetCompanyId(out long companyId)
    {
        companyId = _tenantContext.CompanyId ?? 0;
        return companyId > 0;
    }
}
