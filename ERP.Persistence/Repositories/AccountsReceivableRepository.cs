using ERP.Modules.Accounting.Application.Repositories;
using Microsoft.EntityFrameworkCore;
using AccountingAccountsReceivable = ERP.Modules.Accounting.Domain.AccountsReceivable;
using AccountingAccountsReceivableRepository = ERP.Modules.Accounting.Application.Repositories.IAccountsReceivableRepository;

namespace ERP.Persistence.Repositories;

public sealed class AccountsReceivableRepository : AccountingAccountsReceivableRepository
{
    private readonly ErpDbContext _dbContext;

    public AccountsReceivableRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    async Task AccountingAccountsReceivableRepository.AddAsync(
        AccountingAccountsReceivable receivable,
        CancellationToken cancellationToken)
    {
        await _dbContext.AccountingAccountsReceivables.AddAsync(receivable, cancellationToken);
    }

    async Task<IReadOnlyList<AccountingAccountsReceivable>> AccountingAccountsReceivableRepository.GetByCompanyAsync(
        long companyId,
        CancellationToken cancellationToken)
    {
        var receivables = await _dbContext.AccountingAccountsReceivables
            .Include(r => r.Schedules)
            .Where(r => r.CompanyId == companyId)
            .ToListAsync(cancellationToken);

        return receivables;
    }

}
