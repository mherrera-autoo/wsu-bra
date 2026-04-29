using ERP.Modules.Billing.Application.Repositories;
using ERP.Modules.Billing.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class CreditNoteRepository : ICreditNoteRepository
{
    private readonly ErpDbContext _dbContext;

    public CreditNoteRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(CreditNote creditNote, CancellationToken cancellationToken = default)
    {
        await _dbContext.CreditNotes.AddAsync(creditNote, cancellationToken);
    }

    public Task<CreditNote?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.CreditNotes
            .FirstOrDefaultAsync(cn => cn.Id == id, cancellationToken);

    public Task<long?> GetMaxNumberAsync(long companyId, CancellationToken cancellationToken = default)
        => _dbContext.CreditNotes
            .Where(cn => cn.CompanyId == companyId)
            .Select(cn => (long?)cn.Number)
            .MaxAsync(cancellationToken);
}
