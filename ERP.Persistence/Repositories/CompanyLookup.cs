using ERP.Shared.Application;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class CompanyLookup : ICompanyLookup
{
    private readonly ErpDbContext _dbContext;

    public CompanyLookup(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsAsync(long companyId, CancellationToken cancellationToken = default)
        => _dbContext.Companies.AsNoTracking().AnyAsync(company => company.Id == companyId, cancellationToken);
}
