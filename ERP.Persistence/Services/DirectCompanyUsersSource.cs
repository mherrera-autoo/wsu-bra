using ERP.Modules.Identity.Contracts;
using ERP.Modules.Identity.Application.Services;
using ERP.Modules.Identity.Domain;
using ERP.Modules.MasterData.Contracts;
using ERP.Persistence.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Services;

public sealed class DirectCompanyUsersSource : ICompanyAccessSource
{
    private readonly ErpDbContext _dbContext;

    public DirectCompanyUsersSource(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public bool Supports(OrganizationType accountType) => true;

    public async Task<IReadOnlyCollection<CompanyAccessCandidate>> GetCandidatesAsync(
        long userId,
        long organizationId,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            return Array.Empty<CompanyAccessCandidate>();
        }

        return await _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser => companyUser.UserId == userId && companyUser.Status == CompanyUserStatus.Active)
            .Select(companyUser => new CompanyAccessCandidate(
                companyUser.CompanyPublicId,
                CompanyAccessSource.DirectMembership))
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
