using ERP.Api.Authorization;
using ERP.Api.Contracts.Accounting;
using ERP.Modules.Accounting.Application.Repositories;
using ERP.Modules.Accounting.Application.Services;
using ERP.Modules.Accounting.Contracts;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/accounting/journal-entries")]
public sealed class AccountingJournalController : ControllerBase
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly AccountingPostingService _postingService;
    private readonly ITenantContext _tenantContext;

    public AccountingJournalController(
        IJournalEntryRepository journalEntryRepository,
        AccountingPostingService postingService,
        ITenantContext tenantContext)
    {
        _journalEntryRepository = journalEntryRepository;
        _postingService = postingService;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [RequireCompanyPermission(PermissionKeys.Accounting.JournalRead)]
    public async Task<ActionResult<IReadOnlyList<JournalEntrySummary>>> List(CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var entries = await _journalEntryRepository.ListByCompanyAsync(companyId, cancellationToken);
        var response = entries.Select(entry => new JournalEntrySummary(
            entry.Id,
            entry.Description,
            entry.EntryDate));
        return Ok(response);
    }

    [HttpGet("{id:long}")]
    [RequireCompanyPermission(PermissionKeys.Accounting.JournalRead)]
    public async Task<ActionResult<JournalEntryDetail>> Get(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var entry = await _journalEntryRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (entry is null)
        {
            return NotFound();
        }

        var response = new JournalEntryDetail(
            entry.Id,
            entry.Description,
            entry.EntryDate,
            entry.Lines.Select(line => new JournalEntryLineDetail(
                line.AccountId,
                line.Debit,
                line.Credit)).ToList());
        return Ok(response);
    }

    [HttpPost("adjustments")]
    [RequireCompanyPermission(PermissionKeys.Accounting.JournalAdjustmentsWrite)]
    public async Task<IActionResult> CreateAdjustment(CreateJournalEntryRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var lines = request.Lines
            .Select(line => new AccountingPostRequestedLine(
                line.AccountId,
                line.Debit,
                line.Credit,
                null,
                null))
            .ToList();

        var postRequest = new AccountingPostRequested(
            Guid.NewGuid().ToString("N"),
            "Accounting",
            "Adjustment",
            Guid.NewGuid().ToString("N"),
            "GENERAL",
            DateTime.UtcNow.Date,
            request.Reference,
            lines);

        var result = await _postingService.PostAsync(postRequest, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    private bool TryGetCompanyId(out long companyId)
    {
        companyId = _tenantContext.CompanyId ?? 0;
        return companyId > 0;
    }
}
