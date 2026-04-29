using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;

namespace ERP.Persistence.Repositories;

public sealed class ControlledSubstanceLedgerAuditRepository : IControlledSubstanceLedgerAuditRepository
{
    private readonly ErpDbContext _dbContext;

    public ControlledSubstanceLedgerAuditRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ControlledSubstanceLedgerAudit audit, CancellationToken cancellationToken = default)
    {
        await _dbContext.ControlledSubstanceLedgerAudits.AddAsync(audit, cancellationToken);
    }
}
