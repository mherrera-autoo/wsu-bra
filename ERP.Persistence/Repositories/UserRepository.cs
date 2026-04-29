using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Contracts;
using ERP.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly ErpDbContext _dbContext;

    public UserRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByEmailAsync(Guid companyPublicId, string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return _dbContext.Users
            .FirstOrDefaultAsync(
                user => user.Email == normalizedEmail
                    && user.CompanyUsers.Any(companyUser => companyUser.CompanyPublicId == companyPublicId),
                cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return _dbContext.Users
            .FirstOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);
    }

    public Task<User?> GetByIdAsync(long userId, CancellationToken cancellationToken = default)
        => _dbContext.Users
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);

    public Task<User?> GetByPublicIdAsync(Guid userPublicId, CancellationToken cancellationToken = default)
        => _dbContext.Users
            .FirstOrDefaultAsync(user => user.PublicId == userPublicId, cancellationToken);

    public async Task<IReadOnlyList<UserWithProfileSummary>> GetByPublicIdsWithProfileAsync(
        IReadOnlyCollection<Guid> userPublicIds,
        CancellationToken cancellationToken = default)
    {
        if (userPublicIds.Count == 0)
        {
            return Array.Empty<UserWithProfileSummary>();
        }

        return await (
            from user in _dbContext.Users
            join profile in _dbContext.UserProfiles on user.Id equals profile.UserId into profileGroup
            from profile in profileGroup.DefaultIfEmpty()
            where userPublicIds.Contains(user.PublicId)
            select new UserWithProfileSummary(
                user.PublicId,
                profile != null ? profile.Email : user.Email,
                profile != null ? profile.FirstName : null,
                profile != null ? profile.LastName : null,
                profile != null ? profile.DisplayName : null))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserWithProfileSummary>> GetAllAsync(CancellationToken cancellationToken = default)
        => await (
            from user in _dbContext.Users
            join profile in _dbContext.UserProfiles on user.Id equals profile.UserId into profileGroup
            from profile in profileGroup.DefaultIfEmpty()
            orderby profile != null ? profile.Email : user.Email
            select new UserWithProfileSummary(
                user.PublicId,
                profile != null ? profile.Email : user.Email,
                profile != null ? profile.FirstName : null,
                profile != null ? profile.LastName : null,
                profile != null ? profile.DisplayName : null))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<User>> GetByCompanyAsync(Guid companyPublicId, CancellationToken cancellationToken = default)
        => await (
            from user in _dbContext.Users
            join profile in _dbContext.UserProfiles on user.Id equals profile.UserId into profileGroup
            from profile in profileGroup.DefaultIfEmpty()
            where user.CompanyUsers.Any(companyUser => companyUser.CompanyPublicId == companyPublicId)
            orderby profile != null ? profile.Email : user.Email
            select user)
            .ToListAsync(cancellationToken);

    public Task<bool> EmailExistsAsync(Guid companyPublicId, string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return _dbContext.Users.AnyAsync(
            user => user.Email == normalizedEmail
                && user.CompanyUsers.Any(companyUser => companyUser.CompanyPublicId == companyPublicId),
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetRoleNamesAsync(long userId, CancellationToken cancellationToken = default)
    {
        var companyPublicId = await _dbContext.CompanyUsers
            .Where(companyUser => companyUser.UserId == userId && companyUser.Status == CompanyUserStatus.Active)
            .OrderByDescending(companyUser => companyUser.UpdatedAt ?? companyUser.CreatedAt)
            .ThenBy(companyUser => companyUser.CompanyPublicId)
            .Select(companyUser => (Guid?)companyUser.CompanyPublicId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!companyPublicId.HasValue)
        {
            return Array.Empty<string>();
        }

        var roles = _dbContext.RoleAssignments
            .Where(roleAssignment => 
                roleAssignment.UserId == userId 
                && roleAssignment.Status == RoleAssignmentStatus.Active
                && (roleAssignment.ScopeType == RoleAssignmentScopeType.Platform 
                    || (roleAssignment.ScopeType == RoleAssignmentScopeType.Company 
                        && roleAssignment.CompanyPublicId == companyPublicId.Value)))
            .Select(roleAssignment => roleAssignment.Role.Name);

        return await roles
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetRoleNamesAsync(
        long userId,
        Guid companyPublicId,
        CancellationToken cancellationToken = default)
    {
        if (companyPublicId == Guid.Empty)
        {
            return Array.Empty<string>();
        }

        var roles = _dbContext.RoleAssignments
            .Where(roleAssignment => 
                roleAssignment.UserId == userId 
                && roleAssignment.Status == RoleAssignmentStatus.Active
                && (roleAssignment.ScopeType == RoleAssignmentScopeType.Platform 
                    || (roleAssignment.ScopeType == RoleAssignmentScopeType.Company 
                        && roleAssignment.CompanyPublicId == companyPublicId)))
            .Select(roleAssignment => roleAssignment.Role.Name);

        return await roles
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetPermissionCodesAsync(long userId, CancellationToken cancellationToken = default)
    {
        var companyPublicId = await _dbContext.CompanyUsers
            .Where(companyUser => companyUser.UserId == userId && companyUser.Status == CompanyUserStatus.Active)
            .OrderByDescending(companyUser => companyUser.UpdatedAt ?? companyUser.CreatedAt)
            .ThenBy(companyUser => companyUser.CompanyPublicId)
            .Select(companyUser => (Guid?)companyUser.CompanyPublicId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!companyPublicId.HasValue)
        {
            return Array.Empty<string>();
        }

        var permissions = _dbContext.RoleAssignments
            .Where(roleAssignment => 
                roleAssignment.UserId == userId 
                && roleAssignment.Status == RoleAssignmentStatus.Active
                && (roleAssignment.ScopeType == RoleAssignmentScopeType.Platform 
                    || (roleAssignment.ScopeType == RoleAssignmentScopeType.Company 
                        && roleAssignment.CompanyPublicId == companyPublicId.Value)))
            .SelectMany(roleAssignment => roleAssignment.Role.RolePermissions.Select(rolePermission => rolePermission.Permission.Code));

        return await permissions
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetPermissionCodesAsync(
        long userId,
        Guid companyPublicId,
        CancellationToken cancellationToken = default)
    {
        if (companyPublicId == Guid.Empty)
        {
            return Array.Empty<string>();
        }

        var permissions = _dbContext.RoleAssignments
            .Where(roleAssignment => 
                roleAssignment.UserId == userId 
                && roleAssignment.Status == RoleAssignmentStatus.Active
                && (roleAssignment.ScopeType == RoleAssignmentScopeType.Platform 
                    || (roleAssignment.ScopeType == RoleAssignmentScopeType.Company 
                        && roleAssignment.CompanyPublicId == companyPublicId)))
            .SelectMany(roleAssignment => roleAssignment.Role.RolePermissions.Select(rolePermission => rolePermission.Permission.Code));

        return await permissions
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
        => _dbContext.Users.AddAsync(user, cancellationToken).AsTask();

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
