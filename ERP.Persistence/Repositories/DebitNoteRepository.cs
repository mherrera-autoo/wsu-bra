using ERP.Modules.Billing.Application.Repositories;
using ERP.Modules.Billing.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class DebitNoteRepository : IDebitNoteRepository
{
    private readonly ErpDbContext _dbContext;

    public DebitNoteRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(DebitNote debitNote, CancellationToken cancellationToken = default)
    {
        await _dbContext.DebitNotes.AddAsync(debitNote, cancellationToken);
    }

    public Task<DebitNote?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.DebitNotes
            .FirstOrDefaultAsync(dn => dn.Id == id, cancellationToken);

    public Task<long?> GetMaxNumberAsync(long companyId, CancellationToken cancellationToken = default)
        => _dbContext.DebitNotes
            .Where(dn => dn.CompanyId == companyId)
            .Select(dn => (long?)dn.Number)
            .MaxAsync(cancellationToken);
}
