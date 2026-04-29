using ERP.Modules.Accounting.Domain;

namespace ERP.Modules.Accounting.Application.Repositories;

public interface IAccountingPeriodRepository
{
    Task<AccountingPeriod?> GetByMonthAsync(long companyId, int year, int month, CancellationToken cancellationToken = default);
    Task<AccountingPeriod?> GetOpenByDateAsync(long companyId, DateTime entryDate, CancellationToken cancellationToken = default);
    Task AddAsync(AccountingPeriod period, CancellationToken cancellationToken = default);
}
