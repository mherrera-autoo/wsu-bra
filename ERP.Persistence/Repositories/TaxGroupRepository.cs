using ERP.Modules.Tax.Application.Repositories;
using ERP.Modules.Tax.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class TaxGroupRepository : ITaxGroupRepository
{
    private readonly ErpDbContext _dbContext;

    public TaxGroupRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(TaxGroup taxGroup, CancellationToken cancellationToken = default)
    {
        await _dbContext.TaxGroups.AddAsync(taxGroup, cancellationToken);
    }

    public Task<TaxGroup?> GetByIdWithRulesAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.TaxGroups
            .Include(group => group.Rules)
            .FirstOrDefaultAsync(group => group.Id == id, cancellationToken);
}
