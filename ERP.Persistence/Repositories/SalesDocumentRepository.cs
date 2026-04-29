using ERP.Modules.Sales.Application.Repositories;
using ERP.Modules.Sales.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class SalesDocumentRepository : ISalesDocumentRepository
{
    private readonly ErpDbContext _dbContext;

    public SalesDocumentRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(SalesDocument document, CancellationToken cancellationToken = default)
    {
        await _dbContext.SalesDocuments.AddAsync(document, cancellationToken);
    }

    public Task<SalesDocument?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.SalesDocuments
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<IReadOnlyList<SalesDocument>> ListAsync(long companyId, CancellationToken cancellationToken = default)
        => await _dbContext.SalesDocuments.AsNoTracking()
            .Where(doc => doc.CompanyId == companyId)
            .OrderByDescending(doc => doc.CreatedAt)
            .ToListAsync(cancellationToken);
}
