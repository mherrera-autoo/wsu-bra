namespace ERP.Api.Contracts.Reports;

public sealed record ReportQuery(
    long CompanyId,
    DateTime? From,
    DateTime? To,
    ReportFormat? Format);

public enum ReportFormat
{
    Json,
    Csv
}
