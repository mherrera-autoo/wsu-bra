using ERP.Modules.Identity.Contracts;
using ERP.Modules.Identity.Application.Services;
using ERP.Modules.MasterData.Contracts;
using ERP.Persistence.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Services;

public sealed class HoldingOrganizationCompaniesSource : ICompanyAccessSource
{
    private readonly ErpDbContext _dbContext;

    public HoldingOrganizationCompaniesSource(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public bool Supports(OrganizationType accountType) => accountType == OrganizationType.Holding;

    public async Task<IReadOnlyCollection<CompanyAccessCandidate>> GetCandidatesAsync(
        long userId,
        long organizationId,
        CancellationToken cancellationToken = default)
    {
        if (organizationId <= 0)
        {
            return Array.Empty<CompanyAccessCandidate>();
        }

        return await _dbContext.Companies
            .AsNoTracking()
            .Where(company => company.OrganizationId == organizationId)
            .Select(company => new CompanyAccessCandidate(
                company.PublicId,
                CompanyAccessSource.HoldingWorkspace))
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
