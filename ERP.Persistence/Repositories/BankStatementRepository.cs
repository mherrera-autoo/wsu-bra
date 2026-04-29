using ERP.Modules.Cash.Application.Repositories;
using ERP.Modules.Cash.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class BankStatementRepository : IBankStatementRepository
{
    private readonly ErpDbContext _dbContext;

    public BankStatementRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(BankStatement statement, CancellationToken cancellationToken = default)
    {
        await _dbContext.BankStatements.AddAsync(statement, cancellationToken);
    }

    public Task<BankStatement?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.BankStatements
            .Include(s => s.Transactions)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
}
