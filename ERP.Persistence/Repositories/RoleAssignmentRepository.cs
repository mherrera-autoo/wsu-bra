using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Domain;
using ERP.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public class RoleAssignmentRepository : IRoleAssignmentRepository
{
    private readonly ErpDbContext _dbContext;

    public RoleAssignmentRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(RoleAssignment roleAssignment, CancellationToken cancellationToken = default)
    {
        await _dbContext.RoleAssignments.AddAsync(roleAssignment, cancellationToken);
    }

public async Task<RoleAssignment?> GetAsync(
        long userId, 
        long roleId, 
        RoleAssignmentScopeType scopeType, 
        long? organizationId = null, 
        Guid? companyPublicId = null, 
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RoleAssignments
            .FirstOrDefaultAsync(ra => 
                ra.UserId == userId && 
                ra.RoleId == roleId && 
                ra.ScopeType == scopeType &&
                ra.OrganizationId == organizationId &&
                ra.CompanyPublicId == companyPublicId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<RoleAssignment>> GetByUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RoleAssignments
            .Where(ra => ra.UserId == userId)
            .ToListAsync(cancellationToken);
    }

public async Task<IReadOnlyList<RoleAssignment>> GetByOrganizationAsync(long organizationId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RoleAssignments
            .Where(ra => ra.OrganizationId == organizationId && ra.ScopeType == RoleAssignmentScopeType.Organization)
            .ToListAsync(cancellationToken);
    }

public async Task<IReadOnlyList<RoleAssignment>> GetByCompanyAsync(Guid companyPublicId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RoleAssignments
            .Where(ra => ra.CompanyPublicId == companyPublicId && ra.ScopeType == RoleAssignmentScopeType.Company)
            .ToListAsync(cancellationToken);
    }

public async Task<bool> ExistsAsync(
        long userId, 
        long roleId, 
        RoleAssignmentScopeType scopeType, 
        long? organizationId = null, 
        Guid? companyPublicId = null, 
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RoleAssignments
            .AnyAsync(ra => 
                ra.UserId == userId && 
                ra.RoleId == roleId && 
                ra.ScopeType == scopeType &&
                ra.OrganizationId == organizationId &&
                ra.CompanyPublicId == companyPublicId,
                cancellationToken);
    }

    public async Task UpdateAsync(RoleAssignment roleAssignment, CancellationToken cancellationToken = default)
    {
        _dbContext.RoleAssignments.Update(roleAssignment);
        await Task.CompletedTask;
    }
}
