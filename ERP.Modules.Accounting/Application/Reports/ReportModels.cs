namespace ERP.Modules.Accounting.Application.Reports;

public sealed record ReportLine(long AccountId, string AccountCode, string AccountName, decimal Amount);

public sealed record BalanceSheetSection(string Name, IReadOnlyList<ReportLine> Lines, decimal Total);

public sealed record BalanceSheetReport(
    DateTime? From,
    DateTime? To,
    BalanceSheetSection Assets,
    BalanceSheetSection Liabilities,
    BalanceSheetSection Equity,
    decimal TotalLiabilitiesAndEquity);

public sealed record IncomeStatementReport(
    DateTime? From,
    DateTime? To,
    BalanceSheetSection Revenues,
    BalanceSheetSection Expenses,
    decimal NetIncome);

public sealed record CashFlowReport(
    DateTime? From,
    DateTime? To,
    IReadOnlyList<ReportLine> CashAccounts,
    decimal NetCashChange);

public sealed record GeneralLedgerLine(
    DateTime EntryDate,
    long JournalEntryId,
    string? Description,
    decimal Debit,
    decimal Credit,
    decimal Balance);

public sealed record GeneralLedgerAccount(
    long AccountId,
    string AccountCode,
    string AccountName,
    IReadOnlyList<GeneralLedgerLine> Lines,
    decimal TotalDebit,
    decimal TotalCredit,
    decimal EndingBalance);

public sealed record GeneralLedgerReport(
    DateTime? From,
    DateTime? To,
    IReadOnlyList<GeneralLedgerAccount> Accounts);
