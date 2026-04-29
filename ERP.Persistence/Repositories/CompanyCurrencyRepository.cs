using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class CompanyCurrencyRepository : ICompanyCurrencyRepository
{
    private readonly ErpDbContext _dbContext;

    public CompanyCurrencyRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(CompanyCurrency companyCurrency, CancellationToken cancellationToken = default)
    {
        await _dbContext.CompanyCurrencies.AddAsync(companyCurrency, cancellationToken);
    }

    public Task<CompanyCurrency?> GetAsync(long companyId, long currencyId, CancellationToken cancellationToken = default)
        => _dbContext.CompanyCurrencies
            .Include(companyCurrency => companyCurrency.Currency)
            .FirstOrDefaultAsync(
                companyCurrency => companyCurrency.CompanyId == companyId && companyCurrency.CurrencyId == currencyId,
                cancellationToken);

    public async Task<IReadOnlyList<CompanyCurrency>> ListByCompanyAsync(long companyId, bool activeOnly, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.CompanyCurrencies
            .Include(companyCurrency => companyCurrency.Currency)
            .Where(companyCurrency => companyCurrency.CompanyId == companyId);
        if (activeOnly)
        {
            query = query.Where(companyCurrency => companyCurrency.IsActive);
        }

        return await query
            .OrderBy(companyCurrency => companyCurrency.CurrencyId)
            .ToListAsync(cancellationToken);
    }

    public Task<CompanyCurrency?> GetDefaultAsync(long companyId, CancellationToken cancellationToken = default)
        => _dbContext.CompanyCurrencies
            .Include(companyCurrency => companyCurrency.Currency)
            .FirstOrDefaultAsync(
                companyCurrency => companyCurrency.CompanyId == companyId && companyCurrency.IsDefault,
                cancellationToken);

    public Task<bool> ExistsAsync(long companyId, long currencyId, CancellationToken cancellationToken = default)
        => _dbContext.CompanyCurrencies.AnyAsync(
            companyCurrency => companyCurrency.CompanyId == companyId && companyCurrency.CurrencyId == currencyId,
            cancellationToken);

    public void Remove(CompanyCurrency companyCurrency)
    {
        _dbContext.CompanyCurrencies.Remove(companyCurrency);
    }
}
