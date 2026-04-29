using ERP.Modules.Accounting.Domain;

namespace ERP.Modules.Accounting.Application.Repositories;

public interface IJournalEntryRepository
{
    Task AddAsync(JournalEntry entry, CancellationToken cancellationToken = default);
    Task<JournalEntry?> GetBySourceAsync(
        long companyId,
        string sourceModule,
        string sourceDocumentId,
        string sourceDocumentType,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JournalEntry>> ListByCompanyAsync(long companyId, CancellationToken cancellationToken = default);
    Task<JournalEntry?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JournalEntry>> ListAsync(long companyId, CancellationToken cancellationToken = default);
    void Remove(JournalEntry entry);
}
