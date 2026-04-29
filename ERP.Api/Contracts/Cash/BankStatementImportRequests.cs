namespace ERP.Api.Contracts.Cash;

public sealed record BankStatementCsvImportRequest(
    long CompanyId,
    long BankAccountId,
    DateTime? StatementDate,
    decimal? StartingBalance,
    decimal? EndingBalance,
    string CsvContent);
