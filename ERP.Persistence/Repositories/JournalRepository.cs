using ERP.Modules.Accounting.Application.Repositories;
using ERP.Modules.Accounting.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class JournalRepository : IJournalRepository
{
    private readonly ErpDbContext _dbContext;

    public JournalRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Journal?> GetByCodeAsync(long companyId, string code, CancellationToken cancellationToken = default)
        => _dbContext.Journals.FirstOrDefaultAsync(
            journal => journal.CompanyId == companyId && journal.Code == code,
            cancellationToken);

    public async Task AddAsync(Journal journal, CancellationToken cancellationToken = default)
    {
        await _dbContext.Journals.AddAsync(journal, cancellationToken);
    }
}
