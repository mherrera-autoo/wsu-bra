using ERP.Modules.Accounting.Domain;

namespace ERP.Modules.Accounting.Application.Repositories;

public interface IJournalRepository
{
    Task<Journal?> GetByCodeAsync(long companyId, string code, CancellationToken cancellationToken = default);
    Task AddAsync(Journal journal, CancellationToken cancellationToken = default);
}
