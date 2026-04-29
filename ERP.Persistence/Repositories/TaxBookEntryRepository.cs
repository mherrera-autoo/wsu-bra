using ERP.Modules.Accounting.Application.Repositories;
using ERP.Modules.Accounting.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class TaxBookEntryRepository : ITaxBookEntryRepository
{
    private readonly ErpDbContext _dbContext;

    public TaxBookEntryRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddRangeAsync(IEnumerable<TaxBookEntry> entries, CancellationToken cancellationToken = default)
    {
        await _dbContext.TaxBookEntries.AddRangeAsync(entries, cancellationToken);
    }

    public async Task<IReadOnlyList<TaxBookEntry>> ListByPeriodAsync(
        long companyId,
        int year,
        int month,
        TaxBookType bookType,
        CancellationToken cancellationToken = default)
        => await _dbContext.TaxBookEntries
            .AsNoTracking()
            .Where(entry =>
                entry.CompanyId == companyId
                && entry.Year == year
                && entry.Month == month
                && entry.BookType == bookType)
            .OrderBy(entry => entry.IssueDate)
            .ToListAsync(cancellationToken);
}
