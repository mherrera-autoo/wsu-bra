using ERP.Api.Authorization;
using ERP.Api.Contracts.Accounting;
using ERP.Modules.Accounting.Application.Reports;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/accounting/audit-logs")]
public sealed class AccountingAuditController : ControllerBase
{
    private readonly AccountingAuditService _auditService;
    private readonly ITenantContext _tenantContext;

    public AccountingAuditController(AccountingAuditService auditService, ITenantContext tenantContext)
    {
        _auditService = auditService;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [RequireCompanyPermission(PermissionKeys.Accounting.JournalRead)]
    public async Task<ActionResult<IReadOnlyList<AccountingAuditLogEntry>>> List(
        [FromQuery] AccountingAuditQuery query,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var logs = await _auditService.GetAccountingAuditLogsAsync(
            companyId,
            query.From,
            query.To,
            query.EntityName,
            cancellationToken);

        return Ok(logs);
    }

    private bool TryGetCompanyId(out long companyId)
    {
        companyId = _tenantContext.CompanyId ?? 0;
        return companyId > 0;
    }
}
