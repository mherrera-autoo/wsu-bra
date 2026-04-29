using ERP.Modules.Cash.Application.Repositories;
using ERP.Modules.Cash.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class BankTransactionRepository : IBankTransactionRepository
{
    private readonly ErpDbContext _dbContext;

    public BankTransactionRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(BankTransaction transaction, CancellationToken cancellationToken = default)
    {
        await _dbContext.BankTransactions.AddAsync(transaction, cancellationToken);
    }

    public Task<BankTransaction?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.BankTransactions.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
}
