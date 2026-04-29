using ERP.Modules.Accounting.Application.Repositories;
using ERP.Modules.Accounting.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class AccountingAutomationRuleRepository : IAccountingAutomationRuleRepository
{
    private readonly ErpDbContext _dbContext;

    public AccountingAutomationRuleRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(AccountingAutomationRule rule, CancellationToken cancellationToken = default)
    {
        await _dbContext.AccountingAutomationRules.AddAsync(rule, cancellationToken);
    }

    public Task<AccountingAutomationRule?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => _dbContext.AccountingAutomationRules
            .FirstOrDefaultAsync(rule => rule.CompanyId == companyId && rule.Id == id, cancellationToken);

    public async Task<IReadOnlyList<AccountingAutomationRule>> ListByCompanyAsync(long companyId, CancellationToken cancellationToken = default)
        => await _dbContext.AccountingAutomationRules
            .AsNoTracking()
            .Where(rule => rule.CompanyId == companyId)
            .OrderBy(rule => rule.Name)
            .ToListAsync(cancellationToken);
}
