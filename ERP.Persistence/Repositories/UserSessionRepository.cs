using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class UserSessionRepository : IUserSessionRepository
{
    private readonly ErpDbContext _dbContext;

    public UserSessionRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(UserSession session, CancellationToken cancellationToken = default)
        => _dbContext.UserSessions.AddAsync(session, cancellationToken).AsTask();

    public Task<UserSession?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken = default)
        => _dbContext.UserSessions
            .FirstOrDefaultAsync(session => session.RefreshTokenHash == refreshTokenHash, cancellationToken);

    public Task<UserSession?> GetByRefreshTokenHashForUpdateAsync(string refreshTokenHash, CancellationToken cancellationToken = default)
    {
        if (_dbContext.Database.IsNpgsql())
        {
            return _dbContext.UserSessions
                .FromSqlInterpolated($"SELECT * FROM iam.\"UserSessions\" WHERE \"RefreshTokenHash\" = {refreshTokenHash} FOR UPDATE")
                .FirstOrDefaultAsync(cancellationToken);
        }

        return _dbContext.UserSessions
            .FirstOrDefaultAsync(session => session.RefreshTokenHash == refreshTokenHash, cancellationToken);
    }

    public async Task<IReadOnlyList<UserSession>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
        => await _dbContext.UserSessions
            .Where(session => session.SessionId == sessionId)
            .OrderByDescending(session => session.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<UserSession>> GetActiveSessionsByUserAsync(long userId, CancellationToken cancellationToken = default)
        => await _dbContext.UserSessions
            .Where(session => session.UserId == userId && session.RevokedAt == null && session.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(session => session.LastSeenAt)
            .ToListAsync(cancellationToken);

    public async Task<bool> RevokeByIdAsync(long sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _dbContext.UserSessions
            .FirstOrDefaultAsync(candidate => candidate.Id == sessionId, cancellationToken);
        if (session is null)
        {
            return false;
        }

        if (session.RevokedAt is null)
        {
            session.Revoke(DateTime.UtcNow);
        }

        return true;
    }

    public async Task<bool> RevokeBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var sessions = await _dbContext.UserSessions
            .Where(candidate => candidate.SessionId == sessionId)
            .ToListAsync(cancellationToken);

        if (sessions.Count == 0)
        {
            return false;
        }

        var now = DateTime.UtcNow;
        foreach (var session in sessions)
        {
            session.Revoke(now);
        }

        return true;
    }

    public async Task<bool> RevokeByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken = default)
    {
        var session = await _dbContext.UserSessions
            .FirstOrDefaultAsync(candidate => candidate.RefreshTokenHash == refreshTokenHash, cancellationToken);
        if (session is null)
        {
            return false;
        }

        if (session.RevokedAt is null)
        {
            session.Revoke(DateTime.UtcNow);
        }

        return true;
    }
}
