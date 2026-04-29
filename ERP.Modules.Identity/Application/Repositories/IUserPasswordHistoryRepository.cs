using ERP.Modules.Identity.Domain;

namespace ERP.Modules.Identity.Application.Repositories;

public interface IUserPasswordHistoryRepository
{
    Task<IReadOnlyList<UserPasswordHistory>> GetRecentAsync(long userId, int count, CancellationToken cancellationToken = default);
    Task AddAsync(UserPasswordHistory history, CancellationToken cancellationToken = default);
    Task PruneAsync(long userId, int keepCount, CancellationToken cancellationToken = default);
}