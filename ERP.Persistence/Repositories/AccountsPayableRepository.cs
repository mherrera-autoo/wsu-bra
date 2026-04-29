using ERP.Modules.Accounting.Application.Repositories;
using Microsoft.EntityFrameworkCore;
using AccountingAccountsPayable = ERP.Modules.Accounting.Domain.AccountsPayable;
using AccountingAccountsPayableRepository = ERP.Modules.Accounting.Application.Repositories.IAccountsPayableRepository;

namespace ERP.Persistence.Repositories;

public sealed class AccountsPayableRepository : AccountingAccountsPayableRepository
{
    private readonly ErpDbContext _dbContext;

    public AccountsPayableRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    async Task AccountingAccountsPayableRepository.AddAsync(
        AccountingAccountsPayable accountsPayable,
        CancellationToken cancellationToken)
    {
        await _dbContext.AccountingAccountsPayables.AddAsync(accountsPayable, cancellationToken);
    }

    Task<AccountingAccountsPayable?> AccountingAccountsPayableRepository.GetByIdAsync(
        long id,
        CancellationToken cancellationToken)
    {
        return _dbContext.AccountingAccountsPayables
            .Include(ap => ap.Schedules)
            .FirstOrDefaultAsync(ap => ap.Id == id, cancellationToken);
    }

}
