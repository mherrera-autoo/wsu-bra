using ERP.Modules.Identity.Domain;

namespace ERP.Modules.Identity.Application.Repositories;

public interface IUserSessionRepository
{
    Task AddAsync(UserSession session, CancellationToken cancellationToken = default);
    Task<UserSession?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken = default);
    Task<UserSession?> GetByRefreshTokenHashForUpdateAsync(string refreshTokenHash, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserSession>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserSession>> GetActiveSessionsByUserAsync(long userId, CancellationToken cancellationToken = default);
    Task<bool> RevokeBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<bool> RevokeByIdAsync(long sessionId, CancellationToken cancellationToken = default);
    Task<bool> RevokeByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken = default);
}
