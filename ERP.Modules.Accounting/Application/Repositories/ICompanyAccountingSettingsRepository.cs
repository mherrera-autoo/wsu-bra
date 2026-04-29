using ERP.Modules.Accounting.Domain;

namespace ERP.Modules.Accounting.Application.Repositories;

public interface ICompanyAccountingSettingsRepository
{
    Task<CompanyAccountingSettings?> GetAsync(long companyId, CancellationToken cancellationToken = default);
    Task AddAsync(CompanyAccountingSettings settings, CancellationToken cancellationToken = default);
}
