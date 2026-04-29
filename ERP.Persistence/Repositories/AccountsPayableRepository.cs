using ERP.Modules.Accounting.Application.Repositories;
using ERP.Modules.Finance.Application.Repositories;
using Microsoft.EntityFrameworkCore;
using AccountingAccountsPayable = ERP.Modules.Accounting.Domain.AccountsPayable;
using FinanceAccountsPayable = ERP.Modules.Finance.Domain.AccountsPayable;
using AccountingAccountsPayableRepository = ERP.Modules.Accounting.Application.Repositories.IAccountsPayableRepository;
using FinanceAccountsPayableRepository = ERP.Modules.Finance.Application.Repositories.IAccountsPayableRepository;

namespace ERP.Persistence.Repositories;

public sealed class AccountsPayableRepository : AccountingAccountsPayableRepository, FinanceAccountsPayableRepository
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

    async Task FinanceAccountsPayableRepository.AddAsync(
        FinanceAccountsPayable payable,
        CancellationToken cancellationToken)
    {
        await _dbContext.AccountsPayables.AddAsync(payable, cancellationToken);
    }

    Task<FinanceAccountsPayable?> FinanceAccountsPayableRepository.GetByIdAsync(
        long id,
        CancellationToken cancellationToken)
    {
        return _dbContext.AccountsPayables
            .FirstOrDefaultAsync(ap => ap.Id == id, cancellationToken);
    }
}
