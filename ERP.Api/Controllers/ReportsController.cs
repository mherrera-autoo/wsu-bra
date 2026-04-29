using System.Globalization;
using System.Text;
using ERP.Api.Authorization;
using ERP.Api.Contracts.Reports;
using ERP.Modules.Accounting.Application.Reports;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly ReportsService _reportsService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public ReportsController(ReportsService reportsService, ICurrentUserProvider currentUserProvider)
    {
        _reportsService = reportsService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet("balance-sheet")]
    public async Task<IActionResult> GetBalanceSheet([FromQuery] ReportQuery query, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var report = await _reportsService.GetBalanceSheetAsync(companyId, query.From, query.To, cancellationToken);
        return BuildResponse(report, "balance-sheet", query.Format);
    }

    [HttpGet("income-statement")]
    public async Task<IActionResult> GetIncomeStatement([FromQuery] ReportQuery query, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var report = await _reportsService.GetIncomeStatementAsync(companyId, query.From, query.To, cancellationToken);
        return BuildResponse(report, "income-statement", query.Format);
    }

    [HttpGet("cash-flow")]
    public async Task<IActionResult> GetCashFlow([FromQuery] ReportQuery query, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var report = await _reportsService.GetCashFlowAsync(companyId, query.From, query.To, cancellationToken);
        return BuildResponse(report, "cash-flow", query.Format);
    }

    [HttpGet("general-ledger")]
    public async Task<IActionResult> GetGeneralLedger([FromQuery] GeneralLedgerQuery query, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var report = await _reportsService.GetGeneralLedgerAsync(companyId, query.From, query.To, query.AccountId, cancellationToken);
        if (query.Format == ReportFormat.Csv)
        {
            var csv = ToCsv(report);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", "general-ledger.csv");
        }

        return Ok(report);
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

    private IActionResult BuildResponse(BalanceSheetReport report, string fileName, ReportFormat? format)
    {
        if (format == ReportFormat.Csv)
        {
            var csv = ToCsv(report);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"{fileName}.csv");
        }

        return Ok(report);
    }

    private IActionResult BuildResponse(IncomeStatementReport report, string fileName, ReportFormat? format)
    {
        if (format == ReportFormat.Csv)
        {
            var csv = ToCsv(report);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"{fileName}.csv");
        }

        return Ok(report);
    }

    private IActionResult BuildResponse(CashFlowReport report, string fileName, ReportFormat? format)
    {
        if (format == ReportFormat.Csv)
        {
            var csv = ToCsv(report);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"{fileName}.csv");
        }

        return Ok(report);
    }

    private static string ToCsv(BalanceSheetReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Section,AccountCode,AccountName,Amount");
        AppendSection(sb, "Assets", report.Assets);
        AppendSection(sb, "Liabilities", report.Liabilities);
        AppendSection(sb, "Equity", report.Equity);
        sb.AppendLine(string.Join(",", "Total Liabilities & Equity", "", "", FormatAmount(report.TotalLiabilitiesAndEquity)));
        return sb.ToString();
    }

    private static string ToCsv(IncomeStatementReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Section,AccountCode,AccountName,Amount");
        AppendSection(sb, "Revenue", report.Revenues);
        AppendSection(sb, "Expenses", report.Expenses);
        sb.AppendLine(string.Join(",", "Net Income", "", "", FormatAmount(report.NetIncome)));
        return sb.ToString();
    }

    private static string ToCsv(CashFlowReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("AccountCode,AccountName,Amount");
        foreach (var line in report.CashAccounts)
        {
            sb.AppendLine(string.Join(
                ",",
                CsvValue(line.AccountCode),
                CsvValue(line.AccountName),
                FormatAmount(line.Amount)));
        }

        sb.AppendLine(string.Join(",", "Net Cash Change", "", FormatAmount(report.NetCashChange)));
        return sb.ToString();
    }

    private static void AppendSection(StringBuilder sb, string sectionName, BalanceSheetSection section)
    {
        foreach (var line in section.Lines)
        {
            sb.AppendLine(string.Join(
                ",",
                CsvValue(sectionName),
                CsvValue(line.AccountCode),
                CsvValue(line.AccountName),
                FormatAmount(line.Amount)));
        }

        sb.AppendLine(string.Join(
            ",",
            CsvValue($"{sectionName} Total"),
            "",
            "",
            FormatAmount(section.Total)));
    }

    private static string FormatAmount(decimal amount)
        => amount.ToString("0.00", CultureInfo.InvariantCulture);

    private static string CsvValue(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    private static string ToCsv(GeneralLedgerReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("AccountCode,AccountName,EntryDate,JournalEntryId,Description,Debit,Credit,Balance");

        foreach (var account in report.Accounts)
        {
            foreach (var line in account.Lines)
            {
                sb.AppendLine(string.Join(
                    ",",
                    CsvValue(account.AccountCode),
                    CsvValue(account.AccountName),
                    line.EntryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    line.JournalEntryId,
                    CsvValue(line.Description ?? string.Empty),
                    FormatAmount(line.Debit),
                    FormatAmount(line.Credit),
                    FormatAmount(line.Balance)));
            }
        }

        return sb.ToString();
    }
}
