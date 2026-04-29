using ERP.Api.Authorization;
using ERP.Api.Contracts.Cash;
using ERP.Modules.Cash.Application.Services;
using ERP.Modules.Cash.Domain;
using ERP.Shared.Application;
using ERP.Shared.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/cash")]
public sealed class CashController : ControllerBase
{
    private readonly CashApplicationService _cashApplicationService;
    private readonly BankReconciliationService _bankReconciliationService;
    private readonly CashService _cashService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public CashController(
        CashApplicationService cashApplicationService,
        BankReconciliationService bankReconciliationService,
        CashService cashService,
        ICurrentUserProvider currentUserProvider)
    {
        _cashApplicationService = cashApplicationService;
        _bankReconciliationService = bankReconciliationService;
        _cashService = cashService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpPost("receipts/from-sales")]
    public async Task<IActionResult> CreateReceiptFromSales(CreateReceiptRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        if (request.SalesDocumentId is null)
        {
            return BadRequest(new { error = "SalesDocumentId is required." });
        }

        var result = await _cashService.CreateReceiptAsync(
            companyId,
            request.CustomerId,
            request.SalesDocumentId.Value,
            new Money(request.Amount, request.Currency),
            request.ReceivedAt,
            request.Reference,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id, result.Value!.Status });
    }

    [HttpPost("payments")]
    public async Task<IActionResult> CreatePayment(CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var result = await _cashApplicationService.RecordPaymentAsync(
            companyId,
            request.SupplierId,
            request.BankAccountId,
            new Money(request.Amount, request.Currency),
            request.PaidAt,
            request.PayableAccountId,
            request.BankLedgerAccountId,
            request.Reference,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpPost("receipts")]
    public async Task<IActionResult> CreateReceipt(CreateReceiptRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var result = await _cashApplicationService.RecordReceiptAsync(
            companyId,
            request.CustomerId,
            request.BankAccountId,
            new Money(request.Amount, request.Currency),
            request.ReceivedAt,
            request.ReceivableAccountId,
            request.BankLedgerAccountId,
            request.Reference,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpPost("bank-statements")]
    public async Task<IActionResult> CreateBankStatement(CreateBankStatementRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var transactions = request.Transactions.Select(t =>
            new BankStatementTransactionInfo(t.TransactionDate, t.Amount, t.Description, t.Reference));

        var result = await _bankReconciliationService.CreateStatementAsync(
            companyId,
            request.BankAccountId,
            request.StatementDate,
            request.StartingBalance,
            request.EndingBalance,
            transactions,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpPost("bank-statements/import")]
    public async Task<IActionResult> ImportBankStatementCsv(BankStatementCsvImportRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        if (request.StatementDate is null)
        {
            return BadRequest(new { error = "StatementDate is required." });
        }

        if (request.StartingBalance is null)
        {
            return BadRequest(new { error = "StartingBalance is required." });
        }

        if (request.EndingBalance is null)
        {
            return BadRequest(new { error = "EndingBalance is required." });
        }

        var lines = request.CsvContent
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
        if (lines.Count == 0)
        {
            return BadRequest(new { error = "CSV content is empty." });
        }

        var transactions = new List<BankStatementTransactionInfo>();
        foreach (var line in lines.Skip(1))
        {
            var parts = line.Split(',', 4);
            if (parts.Length < 4)
            {
                return BadRequest(new { error = "CSV rows must include TransactionDate, Amount, Description, Reference." });
            }

            if (!DateTime.TryParse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var txDate))
            {
                return BadRequest(new { error = $"Invalid date '{parts[0]}'." });
            }

            if (!decimal.TryParse(parts[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
            {
                return BadRequest(new { error = $"Invalid amount '{parts[1]}'." });
            }

            transactions.Add(new BankStatementTransactionInfo(
                txDate,
                amount,
                parts[2],
                parts[3]));
        }

        var result = await _bankReconciliationService.CreateStatementAsync(
            companyId,
            request.BankAccountId,
            request.StatementDate.Value,
            request.StartingBalance.Value,
            request.EndingBalance.Value,
            transactions,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpPost("bank-statements/reconcile")]
    public async Task<IActionResult> ReconcileBankStatement(ReconcileBankStatementRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var matches = request.Matches.Select(m => new BankTransactionMatch(m.BankTransactionId, m.PaymentId, m.ReceiptId));

        var result = await _bankReconciliationService.ReconcileStatementAsync(
            companyId,
            request.BankStatementId,
            matches,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
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
