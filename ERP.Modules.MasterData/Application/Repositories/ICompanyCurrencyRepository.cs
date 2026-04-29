using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.Application.Repositories;

public interface ICompanyCurrencyRepository
{
    Task AddAsync(CompanyCurrency companyCurrency, CancellationToken cancellationToken = default);
    Task<CompanyCurrency?> GetAsync(long companyId, long currencyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompanyCurrency>> ListByCompanyAsync(long companyId, bool activeOnly, CancellationToken cancellationToken = default);
    Task<CompanyCurrency?> GetDefaultAsync(long companyId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(long companyId, long currencyId, CancellationToken cancellationToken = default);
    void Remove(CompanyCurrency companyCurrency);
}
