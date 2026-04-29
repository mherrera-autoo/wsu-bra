using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class ExpirationAlertRuleRepository : IExpirationAlertRuleRepository
{
    private readonly ErpDbContext _dbContext;

    public ExpirationAlertRuleRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ExpirationAlertRule?> GetByCompanyAsync(long companyId, CancellationToken cancellationToken = default)
        => _dbContext.ExpirationAlertRules.FirstOrDefaultAsync(rule => rule.CompanyId == companyId, cancellationToken);

    public async Task AddAsync(ExpirationAlertRule rule, CancellationToken cancellationToken = default)
        => await _dbContext.ExpirationAlertRules.AddAsync(rule, cancellationToken);

    public Task UpdateAsync(ExpirationAlertRule rule, CancellationToken cancellationToken = default)
    {
        _dbContext.ExpirationAlertRules.Update(rule);
        return Task.CompletedTask;
    }
}
