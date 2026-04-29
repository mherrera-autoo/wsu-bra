using ERP.Modules.Cash.Application.Repositories;
using ERP.Modules.Cash.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class BankAccountRepository : IBankAccountRepository
{
    private readonly ErpDbContext _dbContext;

    public BankAccountRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(BankAccount bankAccount, CancellationToken cancellationToken = default)
    {
        _dbContext.BankAccounts.Add(bankAccount);
        return Task.CompletedTask;
    }

    public Task<BankAccount?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.BankAccounts.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
}
