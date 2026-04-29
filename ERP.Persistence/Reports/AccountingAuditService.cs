using ERP.Modules.Accounting.Application.Reports;
using ERP.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Modules.Accounting.Application.Reports;

public sealed class AccountingAuditService
{
    private static readonly string[] AccountingEntities =
    [
        "Account",
        "AccountingPeriod",
        "CompanyAccountingSettings",
        "Journal",
        "JournalEntry",
        "JournalEntryLine",
        "DteDocument",
        "TaxBookEntry",
        "TaxDeclaration",
        "AccountingAutomationRule"
    ];

    private readonly ErpDbContext _dbContext;

    public AccountingAuditService(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AccountingAuditLogEntry>> GetAccountingAuditLogsAsync(
        long companyId,
        DateTime? from,
        DateTime? to,
        string? entityName,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AuditLogs.AsNoTracking()
            .Where(log => AccountingEntities.Contains(log.EntityName));

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            query = query.Where(log => log.EntityName == entityName);
        }

        if (from.HasValue)
        {
            query = query.Where(log => log.OccurredAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(log => log.OccurredAt <= to.Value);
        }

        var companyToken = $"\"CompanyId\":{companyId}";
        query = query.Where(log => EF.Functions.Like(log.ChangesJson, $"%{companyToken}%"));

        var logs = await query
            .OrderByDescending(log => log.OccurredAt)
            .ToListAsync(cancellationToken);

        return logs.Select(log => new AccountingAuditLogEntry(
            log.OccurredAt,
            log.Action,
            log.EntityName,
            log.EntityId,
            log.ChangesJson,
            log.Source,
            log.UserId)).ToList();
    }
}
