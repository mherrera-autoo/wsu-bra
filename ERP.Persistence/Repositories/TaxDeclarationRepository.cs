using ERP.Modules.Accounting.Application.Repositories;
using ERP.Modules.Accounting.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class TaxDeclarationRepository : ITaxDeclarationRepository
{
    private readonly ErpDbContext _dbContext;

    public TaxDeclarationRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(TaxDeclaration declaration, CancellationToken cancellationToken = default)
    {
        await _dbContext.TaxDeclarations.AddAsync(declaration, cancellationToken);
    }

    public async Task<IReadOnlyList<TaxDeclaration>> ListByCompanyAsync(long companyId, CancellationToken cancellationToken = default)
        => await _dbContext.TaxDeclarations
            .AsNoTracking()
            .Where(declaration => declaration.CompanyId == companyId)
            .OrderByDescending(declaration => declaration.CreatedAt)
            .ToListAsync(cancellationToken);
}
