using ERP.Modules.Users.Application.Repositories;
using ERP.Modules.Users.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class UserProfileRepository : IUserProfileRepository
{
    private readonly ErpDbContext _dbContext;

    public UserProfileRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(UserProfile profile, CancellationToken cancellationToken = default)
    {
        await _dbContext.UserProfiles.AddAsync(profile, cancellationToken);
    }

    public Task<UserProfile?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.UserProfiles.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<UserProfile?> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
        => _dbContext.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

    public Task<bool> ExistsByEmailAsync(string email, long? excludeId = null, CancellationToken cancellationToken = default)
        => _dbContext.UserProfiles.AnyAsync(p =>
            p.Email == email.Trim().ToLowerInvariant()
            && (!excludeId.HasValue || p.Id != excludeId.Value),
            cancellationToken);

    public async Task<IReadOnlyList<UserProfile>> ListAsync(long userId, CancellationToken cancellationToken = default)
        => await _dbContext.UserProfiles.AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Email)
            .ToListAsync(cancellationToken);

    public void Remove(UserProfile profile)
    {
        _dbContext.UserProfiles.Remove(profile);
    }
}
