using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class CompanyUserRepository : ICompanyUserRepository
{
    private readonly ErpDbContext _dbContext;

    public CompanyUserRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CompanyUser?> GetAsync(Guid companyPublicId, long userId, CancellationToken cancellationToken = default)
        => _dbContext.CompanyUsers
            .FirstOrDefaultAsync(
                companyUser => companyUser.CompanyPublicId == companyPublicId && companyUser.UserId == userId,
                cancellationToken);

    public Task<Guid?> GetActiveCompanyPublicIdAsync(long userId, CancellationToken cancellationToken = default)
        => _dbContext.CompanyUsers
            .Where(companyUser => companyUser.UserId == userId && companyUser.Status == CompanyUserStatus.Active)
            .OrderByDescending(companyUser => companyUser.UpdatedAt ?? companyUser.CreatedAt)
            .ThenBy(companyUser => companyUser.CompanyPublicId)
            .Select(companyUser => (Guid?)companyUser.CompanyPublicId)
            .FirstOrDefaultAsync(cancellationToken);

public Task<bool> IsActiveMemberAsync(Guid companyPublicId, long userId, CancellationToken cancellationToken = default)
        => _dbContext.CompanyUsers
            .AnyAsync(companyUser => 
                companyUser.CompanyPublicId == companyPublicId && 
                companyUser.UserId == userId && 
                companyUser.Status == CompanyUserStatus.Active,
                cancellationToken);

    public Task AddAsync(CompanyUser companyUser, CancellationToken cancellationToken = default)
        => _dbContext.CompanyUsers.AddAsync(companyUser, cancellationToken).AsTask();
}
