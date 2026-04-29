using ERP.Modules.Cash.Application.Repositories;
using ERP.Modules.Cash.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class ReceiptRepository : IReceiptRepository
{
    private readonly ErpDbContext _dbContext;

    public ReceiptRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Receipt receipt, CancellationToken cancellationToken = default)
    {
        await _dbContext.Receipts.AddAsync(receipt, cancellationToken);
    }

    public Task<Receipt?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.Receipts.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
}
