using ERP.Modules.Accounting.Application.Repositories;
using ERP.Modules.Accounting.Domain;

namespace ERP.Modules.Accounting.Infrastructure.Repositories;

public sealed class InMemoryJournalEntryRepository : IJournalEntryRepository
{
    private readonly Dictionary<long, List<JournalEntry>> _entriesByCompany = new();
    private long _nextId = 1;

    public Task AddAsync(JournalEntry entry, CancellationToken cancellationToken = default)
    {
        if (entry is null) throw new ArgumentNullException(nameof(entry));
        entry.AssignId(_nextId++);

        if (!_entriesByCompany.TryGetValue(entry.CompanyId, out var list))
        {
            list = new List<JournalEntry>();
            _entriesByCompany[entry.CompanyId] = list;
        }

        list.Add(entry);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<JournalEntry>> ListByCompanyAsync(long companyId, CancellationToken cancellationToken = default)
    {
        _entriesByCompany.TryGetValue(companyId, out var list);
        return Task.FromResult<IReadOnlyList<JournalEntry>>(list ?? new List<JournalEntry>());
    }

    public Task<JournalEntry?> GetBySourceAsync(
        long companyId,
        string sourceModule,
        string sourceDocumentId,
        string sourceDocumentType,
        CancellationToken cancellationToken = default)
    {
        JournalEntry? entry = null;
        if (_entriesByCompany.TryGetValue(companyId, out var list))
        {
            entry = list.Find(candidate =>
                candidate.SourceModule == sourceModule
                && candidate.SourceDocumentId == sourceDocumentId
                && candidate.SourceDocumentType == sourceDocumentType);
        }

        return Task.FromResult(entry);
    }

    public Task<JournalEntry?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default)
    {
        JournalEntry? entry = null;
        if (_entriesByCompany.TryGetValue(companyId, out var list))
        {
            entry = list.Find(candidate => candidate.Id == id);
        }

        return Task.FromResult(entry);
    }

    public Task<IReadOnlyList<JournalEntry>> ListAsync(long companyId, CancellationToken cancellationToken = default)
        => ListByCompanyAsync(companyId, cancellationToken);

    public void Remove(JournalEntry entry)
    {
        if (entry is null) throw new ArgumentNullException(nameof(entry));
        if (_entriesByCompany.TryGetValue(entry.CompanyId, out var list))
        {
            list.Remove(entry);
            if (list.Count == 0)
            {
                _entriesByCompany.Remove(entry.CompanyId);
            }
        }
    }
}
