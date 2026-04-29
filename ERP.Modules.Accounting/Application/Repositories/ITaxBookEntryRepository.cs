using ERP.Modules.Accounting.Domain;

namespace ERP.Modules.Accounting.Application.Repositories;

public interface ITaxBookEntryRepository
{
    Task AddRangeAsync(IEnumerable<TaxBookEntry> entries, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaxBookEntry>> ListByPeriodAsync(long companyId, int year, int month, TaxBookType bookType, CancellationToken cancellationToken = default);
}
