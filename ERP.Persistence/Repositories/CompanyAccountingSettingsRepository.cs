using ERP.Modules.Accounting.Application.Repositories;
using ERP.Modules.Accounting.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class CompanyAccountingSettingsRepository : ICompanyAccountingSettingsRepository
{
    private readonly ErpDbContext _dbContext;

    public CompanyAccountingSettingsRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CompanyAccountingSettings?> GetAsync(long companyId, CancellationToken cancellationToken = default)
        => _dbContext.CompanyAccountingSettings.FirstOrDefaultAsync(
            settings => settings.CompanyId == companyId,
            cancellationToken);

    public async Task AddAsync(CompanyAccountingSettings settings, CancellationToken cancellationToken = default)
    {
        await _dbContext.CompanyAccountingSettings.AddAsync(settings, cancellationToken);
    }
}
