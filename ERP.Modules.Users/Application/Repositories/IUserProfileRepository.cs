using ERP.Modules.Users.Domain;

namespace ERP.Modules.Users.Application.Repositories;

public interface IUserProfileRepository
{
    Task AddAsync(UserProfile profile, CancellationToken cancellationToken = default);
    Task<UserProfile?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<UserProfile?> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, long? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserProfile>> ListAsync(long userId, CancellationToken cancellationToken = default);
    void Remove(UserProfile profile);
}
