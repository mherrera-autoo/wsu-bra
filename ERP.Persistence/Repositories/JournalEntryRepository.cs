using ERP.Modules.Accounting.Application.Repositories;
using ERP.Modules.Accounting.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class JournalEntryRepository : IJournalEntryRepository
{
    private readonly ErpDbContext _dbContext;

    public JournalEntryRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(JournalEntry entry, CancellationToken cancellationToken = default)
    {
        await _dbContext.JournalEntries.AddAsync(entry, cancellationToken);
    }

    public Task<JournalEntry?> GetBySourceAsync(
        long companyId,
        string sourceModule,
        string sourceDocumentId,
        string sourceDocumentType,
        CancellationToken cancellationToken = default)
        => _dbContext.JournalEntries
            .Include(entry => entry.Lines)
            .FirstOrDefaultAsync(entry =>
                entry.CompanyId == companyId
                && entry.SourceModule == sourceModule
                && entry.SourceDocumentId == sourceDocumentId
                && entry.SourceDocumentType == sourceDocumentType,
                cancellationToken);

    public async Task<IReadOnlyList<JournalEntry>> ListByCompanyAsync(long companyId, CancellationToken cancellationToken = default)
        => await _dbContext.JournalEntries.AsNoTracking()
            .Include(entry => entry.Lines)
            .Where(entry => entry.CompanyId == companyId)
            .OrderByDescending(entry => entry.EntryDate)
            .ToListAsync(cancellationToken);

    public Task<JournalEntry?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => _dbContext.JournalEntries
            .Include(entry => entry.Lines)
            .FirstOrDefaultAsync(entry => entry.CompanyId == companyId && entry.Id == id, cancellationToken);

    public Task<IReadOnlyList<JournalEntry>> ListAsync(long companyId, CancellationToken cancellationToken = default)
        => ListByCompanyAsync(companyId, cancellationToken);

    public void Remove(JournalEntry entry)
    {
        _dbContext.JournalEntries.Remove(entry);
    }
}
