using ERP.Modules.Identity.Contracts;
using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class CompanyAccessRepository : ICompanyAccessRepository
{
    private readonly ErpDbContext _dbContext;

    public CompanyAccessRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CompanyAccessSummary>> ListByUserAsync(
        long userId,
        CancellationToken cancellationToken = default)
        => await _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser => companyUser.UserId == userId && companyUser.Status == CompanyUserStatus.Active)
            .Join(
                _dbContext.Companies.AsNoTracking(),
                companyUser => companyUser.CompanyPublicId,
                company => company.PublicId,
                (_, company) => company)
            .OrderBy(company => company.Name)
            .Select(company => new CompanyAccessSummary(
                company.PublicId,
                company.Id,
                company.OrganizationId,
                company.Name,
                true,
                CompanyAccessSource.DirectMembership,
                null))
            .ToListAsync(cancellationToken);
}
