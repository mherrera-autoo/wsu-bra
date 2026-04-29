using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class OrganizationMemberRepository : IOrganizationMemberRepository
{
    private readonly ErpDbContext _dbContext;

    public OrganizationMemberRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<OrganizationMember?> GetAsync(
        long organizationId,
        long userId,
        CancellationToken cancellationToken = default)
        => _dbContext.OrganizationMembers
            .FirstOrDefaultAsync(member => member.OrganizationId == organizationId && member.UserId == userId, cancellationToken);

    public Task<bool> ExistsAsync(
        long organizationId,
        long userId,
        CancellationToken cancellationToken = default)
        => _dbContext.OrganizationMembers
            .AnyAsync(member => member.OrganizationId == organizationId && member.UserId == userId, cancellationToken);

    public Task AddAsync(OrganizationMember member, CancellationToken cancellationToken = default)
        => _dbContext.OrganizationMembers.AddAsync(member, cancellationToken).AsTask();
}
