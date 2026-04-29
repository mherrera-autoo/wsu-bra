using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class CurrencyRepository : ICurrencyRepository
{
    private readonly ErpDbContext _dbContext;

    public CurrencyRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Currency currency, CancellationToken cancellationToken = default)
    {
        await _dbContext.Currencies.AddAsync(currency, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Currency> currencies, CancellationToken cancellationToken = default)
    {
        await _dbContext.Currencies.AddRangeAsync(currencies, cancellationToken);
    }

    public async Task<Currency?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Currencies
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Currency?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Currencies
            .FirstOrDefaultAsync(c => c.Code == code, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, long? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Currencies.Where(c => c.Code == code);
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
        
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Currency>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Currencies
            .OrderBy(c => c.Order)
            .ThenBy(c => c.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Currencies.CountAsync(cancellationToken);
    }
}