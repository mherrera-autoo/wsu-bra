using ERP.Api.Authorization;
using ERP.Api.Contracts.Accounting;
using ERP.Modules.Accounting.Application.Services;
using ERP.Modules.Accounting.Domain;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/accounting/tax-compliance")]
public sealed class AccountingTaxComplianceController : ControllerBase
{
    private readonly TaxComplianceService _taxComplianceService;
    private readonly ITenantContext _tenantContext;

    public AccountingTaxComplianceController(TaxComplianceService taxComplianceService, ITenantContext tenantContext)
    {
        _taxComplianceService = taxComplianceService;
        _tenantContext = tenantContext;
    }

    [HttpPost("dte")]
    [RequireCompanyPermission(PermissionKeys.Accounting.TaxComplianceWrite)]
    public async Task<IActionResult> IngestDte(DteDocumentRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _taxComplianceService.IngestDteAsync(
            companyId,
            request.Folio,
            request.DocumentType,
            request.IssueDate,
            request.CounterpartyTaxId,
            request.NetAmount,
            request.TaxAmount,
            request.TotalAmount,
            request.CurrencyCode,
            request.Source,
            request.AutomationProviderKey,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpGet("dte")]
    [RequireCompanyPermission(PermissionKeys.Accounting.TaxComplianceRead)]
    public async Task<ActionResult<IReadOnlyList<DteDocumentResponse>>> ListDtes(CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var documents = await _taxComplianceService.ListDtesAsync(companyId, cancellationToken);
        var response = documents.Select(document => new DteDocumentResponse(
            document.Id,
            document.Folio,
            document.DocumentType,
            document.IssueDate,
            document.CounterpartyTaxId,
            document.NetAmount,
            document.TaxAmount,
            document.TotalAmount,
            document.CurrencyCode,
            document.Status,
            document.StatusReason,
            document.Source));

        return Ok(response);
    }

    [HttpPost("tax-books/generate")]
    [RequireCompanyPermission(PermissionKeys.Accounting.TaxComplianceWrite)]
    public async Task<ActionResult<IReadOnlyList<TaxBookEntryResponse>>> GenerateTaxBook(
        TaxBookGenerateRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var entries = await _taxComplianceService.GenerateTaxBookAsync(
            companyId,
            request.Year,
            request.Month,
            request.BookType,
            request.AutomationProviderKey,
            cancellationToken);

        var response = entries.Select(entry => new TaxBookEntryResponse(
            entry.Id,
            entry.Year,
            entry.Month,
            entry.BookType,
            entry.Folio,
            entry.DocumentType,
            entry.IssueDate,
            entry.CounterpartyTaxId,
            entry.NetAmount,
            entry.TaxAmount,
            entry.TotalAmount,
            entry.CurrencyCode));

        return Ok(response);
    }

    [HttpGet("tax-books")]
    [RequireCompanyPermission(PermissionKeys.Accounting.TaxComplianceRead)]
    public async Task<ActionResult<IReadOnlyList<TaxBookEntryResponse>>> ListTaxBookEntries(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] TaxBookType bookType,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var entries = await _taxComplianceService.ListTaxBookEntriesAsync(companyId, year, month, bookType, cancellationToken);
        var response = entries.Select(entry => new TaxBookEntryResponse(
            entry.Id,
            entry.Year,
            entry.Month,
            entry.BookType,
            entry.Folio,
            entry.DocumentType,
            entry.IssueDate,
            entry.CounterpartyTaxId,
            entry.NetAmount,
            entry.TaxAmount,
            entry.TotalAmount,
            entry.CurrencyCode));

        return Ok(response);
    }

    [HttpPost("declarations")]
    [RequireCompanyPermission(PermissionKeys.Accounting.TaxComplianceWrite)]
    public async Task<ActionResult> CreateDeclaration(TaxDeclarationRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _taxComplianceService.CreateDeclarationAsync(
            companyId,
            request.Year,
            request.Month,
            request.DeclarationType,
            request.Payload,
            request.AutomationProviderKey,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpGet("declarations")]
    [RequireCompanyPermission(PermissionKeys.Accounting.TaxComplianceRead)]
    public async Task<ActionResult<IReadOnlyList<TaxDeclarationResponse>>> ListDeclarations(CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var declarations = await _taxComplianceService.ListDeclarationsAsync(companyId, cancellationToken);
        var response = declarations.Select(declaration => new TaxDeclarationResponse(
            declaration.Id,
            declaration.Year,
            declaration.Month,
            declaration.DeclarationType,
            declaration.Status,
            declaration.Payload,
            declaration.ExternalReference,
            declaration.StatusMessage));

        return Ok(response);
    }

    [HttpPost("rules")]
    [RequireCompanyPermission(PermissionKeys.Accounting.AutomationRulesManage)]
    public async Task<ActionResult> CreateRule(AccountingRuleRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _taxComplianceService.CreateRuleAsync(
            companyId,
            request.Name,
            request.Trigger,
            request.DteDocumentType,
            request.SourceModule,
            request.SourceDocumentType,
            request.CounterpartyTaxId,
            request.DebitAccountId,
            request.CreditAccountId,
            request.TaxCode,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpGet("rules")]
    [RequireCompanyPermission(PermissionKeys.Accounting.AutomationRulesManage)]
    public async Task<ActionResult<IReadOnlyList<AccountingRuleResponse>>> ListRules(CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var rules = await _taxComplianceService.ListRulesAsync(companyId, cancellationToken);
        var response = rules.Select(rule => new AccountingRuleResponse(
            rule.Id,
            rule.Name,
            rule.Trigger,
            rule.DteDocumentType,
            rule.SourceModule,
            rule.SourceDocumentType,
            rule.CounterpartyTaxId,
            rule.DebitAccountId,
            rule.CreditAccountId,
            rule.TaxCode,
            rule.IsActive,
            rule.ProposedBySystem));

        return Ok(response);
    }

    [HttpPost("rules/proposals")]
    [RequireCompanyPermission(PermissionKeys.Accounting.AutomationRulesManage)]
    public async Task<ActionResult<IReadOnlyList<AccountingRuleResponse>>> ProposeRules(
        RuleProposalRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }
        var rules = await _taxComplianceService.ProposeRulesAsync(
            companyId,
            request.DteDocumentType,
            request.SourceModule,
            request.SourceDocumentType,
            cancellationToken);
        var response = rules.Select(rule => new AccountingRuleResponse(
            rule.Id,
            rule.Name,
            rule.Trigger,
            rule.DteDocumentType,
            rule.SourceModule,
            rule.SourceDocumentType,
            rule.CounterpartyTaxId,
            rule.DebitAccountId,
            rule.CreditAccountId,
            rule.TaxCode,
            rule.IsActive,
            rule.ProposedBySystem));

        return Ok(response);
    }

    private bool TryGetCompanyId(out long companyId)
    {
        companyId = _tenantContext.CompanyId ?? 0;
        return companyId > 0;
    }
}
