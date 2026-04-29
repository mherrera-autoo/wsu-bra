using ERP.Modules.Identity.Contracts;
using ERP.Modules.Identity.Application.Services;
using ERP.Modules.MasterData.Contracts;
using ERP.Persistence.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Services;

public sealed class MultiCompanyPortfolioSource : ICompanyAccessSource
{
    private readonly ErpDbContext _dbContext;

    public MultiCompanyPortfolioSource(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public bool Supports(OrganizationType accountType) => accountType == OrganizationType.MultiCompany;

    public async Task<IReadOnlyCollection<CompanyAccessCandidate>> GetCandidatesAsync(
        long userId,
        long organizationId,
        CancellationToken cancellationToken = default)
    {
        if (organizationId <= 0)
        {
            return Array.Empty<CompanyAccessCandidate>();
        }

        return await _dbContext.CompanyLinks
            .AsNoTracking()
            .Where(link => link.OrganizationId == organizationId)
            .Select(link => new CompanyAccessCandidate(
                link.CompanyPublicId,
                CompanyAccessSource.PortfolioLink,
                link.AccessType))
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
