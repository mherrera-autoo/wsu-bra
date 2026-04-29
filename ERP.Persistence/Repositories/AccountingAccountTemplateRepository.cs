using ERP.Modules.Accounting.Application.Repositories;
using ERP.Modules.Accounting.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class AccountingAccountTemplateRepository : IAccountingAccountTemplateRepository
{
    private readonly ErpDbContext _dbContext;

    public AccountingAccountTemplateRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AccountingAccountTemplate>> ListByVersionAsync(
        string version,
        CancellationToken cancellationToken = default)
        => await _dbContext.AccountingAccountTemplates
            .Where(template => template.Version == version)
            .OrderBy(template => template.SortOrder)
            .ThenBy(template => template.Code)
            .ToListAsync(cancellationToken);

    public Task AddAsync(AccountingAccountTemplate template, CancellationToken cancellationToken = default)
        => _dbContext.AccountingAccountTemplates.AddAsync(template, cancellationToken).AsTask();
}
