using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.Application.Repositories;

public interface ICurrencyRepository
{
    Task AddAsync(Currency currency, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Currency> currencies, CancellationToken cancellationToken = default);
    Task<Currency?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Currency?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, long? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Currency>> ListAsync(CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}