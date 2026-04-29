using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class ControlledSubstanceLedgerRepository : IControlledSubstanceLedgerRepository
{
    private readonly ErpDbContext _dbContext;

    public ControlledSubstanceLedgerRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ControlledSubstanceLedger?> GetByCompanyAsync(long companyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ControlledSubstanceLedgers
            .FirstOrDefaultAsync(ledger => ledger.CompanyId == companyId, cancellationToken);
    }

    public async Task AddAsync(ControlledSubstanceLedger ledger, CancellationToken cancellationToken = default)
    {
        await _dbContext.ControlledSubstanceLedgers.AddAsync(ledger, cancellationToken);
    }
}
