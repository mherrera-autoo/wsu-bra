using ERP.Modules.Identity.Domain;

namespace ERP.Modules.Identity.Application.Repositories;

public interface IPermissionRepository
{
    Task AddAsync(Permission permission, CancellationToken cancellationToken = default);
    Task<Permission?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Permission?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, long? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, long? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Permission>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Permission>> ListByIdsAsync(IReadOnlyCollection<long> ids, CancellationToken cancellationToken = default);
    void Remove(Permission permission);
}
