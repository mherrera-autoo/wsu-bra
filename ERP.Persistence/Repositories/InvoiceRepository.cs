using ERP.Modules.Billing.Application.Repositories;
using ERP.Modules.Billing.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly ErpDbContext _dbContext;

    public InvoiceRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        await _dbContext.Invoices.AddAsync(invoice, cancellationToken);
    }

    public Task<Invoice?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.Invoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public Task<long?> GetMaxNumberAsync(long companyId, CancellationToken cancellationToken = default)
        => _dbContext.Invoices
            .Where(i => i.CompanyId == companyId)
            .Select(i => (long?)i.Number)
            .MaxAsync(cancellationToken);
}
