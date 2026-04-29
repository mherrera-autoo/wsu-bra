using ERP.Modules.Accounting.Domain;
using ERP.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Modules.Accounting.Application.Reports;

public sealed class ReportsService
{
    private readonly ErpDbContext _dbContext;

    public ReportsService(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BalanceSheetReport> GetBalanceSheetAsync(
        long companyId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken)
    {
        var entries = await GetEntryLines(companyId, from, to, cancellationToken);

        var assetLines = BuildLines(entries, AccountType.Asset);
        var liabilityLines = BuildLines(entries, AccountType.Liability);
        var equityLines = BuildLines(entries, AccountType.Equity);

        var assets = new BalanceSheetSection("Assets", assetLines, assetLines.Sum(x => x.Amount));
        var liabilities = new BalanceSheetSection("Liabilities", liabilityLines, liabilityLines.Sum(x => x.Amount));
        var equity = new BalanceSheetSection("Equity", equityLines, equityLines.Sum(x => x.Amount));

        return new BalanceSheetReport(from, to, assets, liabilities, equity, liabilities.Total + equity.Total);
    }

    public async Task<IncomeStatementReport> GetIncomeStatementAsync(
        long companyId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken)
    {
        var entries = await GetEntryLines(companyId, from, to, cancellationToken);

        var revenueLines = BuildLines(entries, AccountType.Revenue);
        var costLines = BuildLines(entries, AccountType.Cost);
        var expenseLines = BuildLines(entries, AccountType.Expense);
        var totalExpenseLines = costLines.Concat(expenseLines).ToList();

        var revenues = new BalanceSheetSection("Revenue", revenueLines, revenueLines.Sum(x => x.Amount));
        var expenses = new BalanceSheetSection("Expenses", totalExpenseLines, totalExpenseLines.Sum(x => x.Amount));

        return new IncomeStatementReport(from, to, revenues, expenses, revenues.Total - expenses.Total);
    }

    public async Task<CashFlowReport> GetCashFlowAsync(
        long companyId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken)
    {
        var entries = await GetEntryLines(companyId, from, to, cancellationToken);

        var cashAccounts = entries
            .Where(entry => entry.AccountType == AccountType.Asset && IsCashAccount(entry))
            .GroupBy(entry => new { entry.AccountId, entry.AccountCode, entry.AccountName })
            .Select(group => new ReportLine(
                group.Key.AccountId,
                group.Key.AccountCode,
                group.Key.AccountName,
                group.Sum(x => x.Debit - x.Credit)))
            .OrderBy(line => line.AccountCode)
            .ToList();

        return new CashFlowReport(from, to, cashAccounts, cashAccounts.Sum(x => x.Amount));
    }

    public async Task<GeneralLedgerReport> GetGeneralLedgerAsync(
        long companyId,
        DateTime? from,
        DateTime? to,
        long? accountId,
        CancellationToken cancellationToken)
    {
        var entries = await GetEntryLines(companyId, from, to, cancellationToken);

        if (accountId.HasValue)
        {
            entries = entries.Where(entry => entry.AccountId == accountId.Value).ToList();
        }

        var accounts = entries
            .GroupBy(entry => new { entry.AccountId, entry.AccountCode, entry.AccountName, entry.AccountType })
            .OrderBy(group => group.Key.AccountCode)
            .Select(group =>
            {
                var ordered = group.OrderBy(line => line.EntryDate).ToList();
                var balance = 0m;
                var lines = new List<GeneralLedgerLine>();

                foreach (var line in ordered)
                {
                    var net = NetAmount(group.Key.AccountType, line.Debit, line.Credit);
                    balance += net;
                    lines.Add(new GeneralLedgerLine(
                        line.EntryDate,
                        line.JournalEntryId,
                        line.Description,
                        line.Debit,
                        line.Credit,
                        balance));
                }

                var totalDebit = ordered.Sum(line => line.Debit);
                var totalCredit = ordered.Sum(line => line.Credit);
                return new GeneralLedgerAccount(
                    group.Key.AccountId,
                    group.Key.AccountCode,
                    group.Key.AccountName,
                    lines,
                    totalDebit,
                    totalCredit,
                    balance);
            })
            .ToList();

        return new GeneralLedgerReport(from, to, accounts);
    }

    private static List<ReportLine> BuildLines(List<EntryLine> entries, AccountType type)
    {
        return entries
            .Where(entry => entry.AccountType == type)
            .GroupBy(entry => new { entry.AccountId, entry.AccountCode, entry.AccountName })
            .Select(group => new ReportLine(
                group.Key.AccountId,
                group.Key.AccountCode,
                group.Key.AccountName,
                group.Sum(x => NetAmount(type, x.Debit, x.Credit))))
            .OrderBy(line => line.AccountCode)
            .ToList();
    }

    private static decimal NetAmount(AccountType type, decimal debit, decimal credit)
        => type is AccountType.Asset or AccountType.Expense or AccountType.Cost ? debit - credit : credit - debit;

    private static bool IsCashAccount(EntryLine entry)
        => entry.AccountCode.StartsWith("CASH", StringComparison.OrdinalIgnoreCase)
           || entry.AccountCode.StartsWith("101", StringComparison.OrdinalIgnoreCase)
           || entry.AccountName.Contains("cash", StringComparison.OrdinalIgnoreCase);

    private async Task<List<EntryLine>> GetEntryLines(
        long companyId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken)
    {
        var query = from line in _dbContext.JournalEntryLines
            join entry in _dbContext.JournalEntries on line.JournalEntryId equals entry.Id
            join account in _dbContext.Accounts on line.AccountId equals account.Id
            where entry.CompanyId == companyId
                && account.CompanyId == companyId
            select new EntryLine(
                entry.EntryDate,
                entry.Id,
                entry.Description,
                account.Id,
                account.Code,
                account.Name,
                account.AccountType,
                line.Debit,
                line.Credit);

        if (from.HasValue)
        {
            query = query.Where(entry => entry.EntryDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(entry => entry.EntryDate <= to.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    private sealed record EntryLine(
        DateTime EntryDate,
        long JournalEntryId,
        string? Description,
        long AccountId,
        string AccountCode,
        string AccountName,
        AccountType AccountType,
        decimal Debit,
        decimal Credit);
}
