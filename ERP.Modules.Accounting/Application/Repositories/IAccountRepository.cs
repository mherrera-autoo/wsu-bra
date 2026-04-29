using ERP.Modules.Accounting.Domain;

namespace ERP.Modules.Accounting.Application.Repositories;

public interface IAccountRepository
{
    Task AddAsync(Account account, CancellationToken cancellationToken = default);
    Task<Account?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Account>> GetByIdsAsync(long companyId, IReadOnlyCollection<long> ids, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(long companyId, string code, long? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> CountAsync(long companyId, CancellationToken cancellationToken = default);
    Task<bool> HasChildrenAsync(long companyId, long accountId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Account>> ListAsync(
        long companyId,
        string? search,
        bool includeInactive,
        bool includeSystem,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Account>> ListAsync(long companyId, CancellationToken cancellationToken = default);
    void Remove(Account account);
}
