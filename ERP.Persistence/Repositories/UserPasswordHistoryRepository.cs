using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class UserPasswordHistoryRepository(ErpDbContext context) : IUserPasswordHistoryRepository
{
    public async Task<IReadOnlyList<UserPasswordHistory>> GetRecentAsync(long userId, int count, CancellationToken cancellationToken = default)
    {
        return await context.UserPasswordHistory
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.CreatedAtUtc)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(UserPasswordHistory history, CancellationToken cancellationToken = default)
    {
        await context.UserPasswordHistory.AddAsync(history, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task PruneAsync(long userId, int keepCount, CancellationToken cancellationToken = default)
    {
        var toDelete = await context.UserPasswordHistory
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.CreatedAtUtc)
            .Skip(keepCount)
            .ToListAsync(cancellationToken);

        if (toDelete.Count > 0)
        {
            context.UserPasswordHistory.RemoveRange(toDelete);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}