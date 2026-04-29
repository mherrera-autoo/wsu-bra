namespace ERP.Api.Contracts.Reports;

public sealed record GeneralLedgerQuery(
    long CompanyId,
    DateTime? From,
    DateTime? To,
    long? AccountId,
    ReportFormat? Format);
