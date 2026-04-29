using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class PermissionRepository : IPermissionRepository
{
    private readonly ErpDbContext _dbContext;

    public PermissionRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(Permission permission, CancellationToken cancellationToken = default)
        => _dbContext.Permissions.AddAsync(permission, cancellationToken).AsTask();

    public Task<Permission?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.Permissions.FirstOrDefaultAsync(permission => permission.Id == id, cancellationToken);

    public Task<Permission?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => _dbContext.Permissions.FirstOrDefaultAsync(permission => permission.Code == code.Trim(), cancellationToken);

    public Task<bool> ExistsByCodeAsync(string code, long? excludeId = null, CancellationToken cancellationToken = default)
        => _dbContext.Permissions.AnyAsync(
            permission => permission.Code == code.Trim()
                && (!excludeId.HasValue || permission.Id != excludeId.Value),
            cancellationToken);

    public Task<bool> ExistsByNameAsync(string name, long? excludeId = null, CancellationToken cancellationToken = default)
        => _dbContext.Permissions.AnyAsync(
            permission => permission.Name == name.Trim()
                && (!excludeId.HasValue || permission.Id != excludeId.Value),
            cancellationToken);

    public async Task<IReadOnlyList<Permission>> ListAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Permissions.AsNoTracking()
            .OrderBy(permission => permission.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Permission>> ListByIdsAsync(IReadOnlyCollection<long> ids, CancellationToken cancellationToken = default)
        => await _dbContext.Permissions.AsNoTracking()
            .Where(permission => ids.Contains(permission.Id))
            .ToListAsync(cancellationToken);

    public void Remove(Permission permission)
    {
        _dbContext.Permissions.Remove(permission);
    }
}
