using ERP.Modules.Tax.Application.Repositories;
using ERP.Modules.Tax.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class TaxRepository : ITaxRepository
{
    private readonly ErpDbContext _dbContext;

    public TaxRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Tax tax, CancellationToken cancellationToken = default)
    {
        await _dbContext.Taxes.AddAsync(tax, cancellationToken);
    }

    public Task<Tax?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.Taxes.FirstOrDefaultAsync(tax => tax.Id == id, cancellationToken);
}
