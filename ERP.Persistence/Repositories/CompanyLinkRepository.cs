using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Domain;
using ERP.Persistence.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class CompanyLinkRepository : ICompanyLinkRepository
{
    private readonly ErpDbContext _dbContext;

    public CompanyLinkRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CompanyLink?> GetAsync(long organizationId, Guid companyPublicId, CancellationToken cancellationToken = default)
        => _dbContext.CompanyLinks
            .FirstOrDefaultAsync(link => link.OrganizationId == organizationId && link.CompanyPublicId == companyPublicId, cancellationToken);

    public Task<bool> ExistsAsync(long organizationId, Guid companyPublicId, CancellationToken cancellationToken = default)
        => _dbContext.CompanyLinks
            .AnyAsync(link => link.OrganizationId == organizationId && link.CompanyPublicId == companyPublicId, cancellationToken);

    public Task AddAsync(CompanyLink link, CancellationToken cancellationToken = default)
        => _dbContext.CompanyLinks.AddAsync(link, cancellationToken).AsTask();
}
