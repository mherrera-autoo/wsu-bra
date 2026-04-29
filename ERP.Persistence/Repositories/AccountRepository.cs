using ERP.Modules.Accounting.Application.Repositories;
using ERP.Modules.Accounting.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly ErpDbContext _dbContext;

    public AccountRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        await _dbContext.Accounts.AddAsync(account, cancellationToken);
    }

    public Task<Account?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => _dbContext.Accounts.FirstOrDefaultAsync(a => a.CompanyId == companyId && a.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Account>> GetByIdsAsync(
        long companyId,
        IReadOnlyCollection<long> ids,
        CancellationToken cancellationToken = default)
        => await _dbContext.Accounts
            .Where(account => account.CompanyId == companyId && ids.Contains(account.Id))
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsByCodeAsync(long companyId, string code, long? excludeId = null, CancellationToken cancellationToken = default)
        => _dbContext.Accounts.AnyAsync(a =>
            a.CompanyId == companyId
            && a.Code == code
            && (!excludeId.HasValue || a.Id != excludeId.Value),
            cancellationToken);

    public Task<int> CountAsync(long companyId, CancellationToken cancellationToken = default)
        => _dbContext.Accounts.CountAsync(account => account.CompanyId == companyId, cancellationToken);

    public Task<bool> HasChildrenAsync(long companyId, long accountId, CancellationToken cancellationToken = default)
        => _dbContext.Accounts.AnyAsync(
            account => account.CompanyId == companyId && account.ParentId == accountId,
            cancellationToken);

    public async Task<IReadOnlyList<Account>> ListAsync(
        long companyId,
        string? search,
        bool includeInactive,
        bool includeSystem,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Accounts.AsNoTracking().Where(account => account.CompanyId == companyId);

        if (!includeInactive)
        {
            query = query.Where(account => account.IsActive);
        }

        if (!includeSystem)
        {
            query = query.Where(account => !account.IsSystemRequired);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(account =>
                EF.Functions.ILike(account.Code, term) || EF.Functions.ILike(account.Name, term));
        }

        return await query
            .OrderBy(account => account.SortOrder)
            .ThenBy(account => account.Code)
            .ToListAsync(cancellationToken);
    }

    public Task<IReadOnlyList<Account>> ListAsync(long companyId, CancellationToken cancellationToken = default)
        => ListAsync(companyId, null, includeInactive: true, includeSystem: true, cancellationToken);

    public void Remove(Account account)
    {
        _dbContext.Accounts.Remove(account);
    }
}
