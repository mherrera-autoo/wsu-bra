using ERP.Modules.Accounting.Application.Repositories;
using ERP.Modules.Accounting.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class AccountingPeriodRepository : IAccountingPeriodRepository
{
    private readonly ErpDbContext _dbContext;

    public AccountingPeriodRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AccountingPeriod?> GetByMonthAsync(long companyId, int year, int month, CancellationToken cancellationToken = default)
        => _dbContext.AccountingPeriods.FirstOrDefaultAsync(
            period => period.CompanyId == companyId && period.Year == year && period.Month == month,
            cancellationToken);

    public Task<AccountingPeriod?> GetOpenByDateAsync(long companyId, DateTime entryDate, CancellationToken cancellationToken = default)
        => _dbContext.AccountingPeriods.FirstOrDefaultAsync(
            period => period.CompanyId == companyId
                && period.Year == entryDate.Year
                && period.Month == entryDate.Month
                && period.Status == AccountingPeriodStatus.Open,
            cancellationToken);

    public async Task AddAsync(AccountingPeriod period, CancellationToken cancellationToken = default)
    {
        await _dbContext.AccountingPeriods.AddAsync(period, cancellationToken);
    }
}
