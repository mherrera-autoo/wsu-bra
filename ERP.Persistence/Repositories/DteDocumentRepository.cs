using ERP.Modules.Accounting.Application.Repositories;
using ERP.Modules.Accounting.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class DteDocumentRepository : IDteDocumentRepository
{
    private readonly ErpDbContext _dbContext;

    public DteDocumentRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(DteDocument document, CancellationToken cancellationToken = default)
    {
        await _dbContext.DteDocuments.AddAsync(document, cancellationToken);
    }

    public Task<DteDocument?> GetByFolioAsync(long companyId, string folio, DteDocumentType documentType, CancellationToken cancellationToken = default)
        => _dbContext.DteDocuments
            .FirstOrDefaultAsync(document =>
                document.CompanyId == companyId
                && document.Folio == folio
                && document.DocumentType == documentType,
                cancellationToken);

    public async Task<IReadOnlyList<DteDocument>> ListByCompanyAsync(long companyId, CancellationToken cancellationToken = default)
        => await _dbContext.DteDocuments
            .AsNoTracking()
            .Where(document => document.CompanyId == companyId)
            .OrderByDescending(document => document.IssueDate)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DteDocument>> ListByPeriodAsync(long companyId, int year, int month, CancellationToken cancellationToken = default)
        => await _dbContext.DteDocuments
            .AsNoTracking()
            .Where(document =>
                document.CompanyId == companyId
                && document.IssueDate.Year == year
                && document.IssueDate.Month == month)
            .OrderByDescending(document => document.IssueDate)
            .ToListAsync(cancellationToken);
}
