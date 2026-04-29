using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;

namespace ERP.Persistence.Repositories;

public sealed class PharmacyCriticalAuditRepository : IPharmacyCriticalAuditRepository
{
    private readonly ErpDbContext _dbContext;

    public PharmacyCriticalAuditRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(PharmacyCriticalAudit audit, CancellationToken cancellationToken = default)
    {
        await _dbContext.PharmacyCriticalAudits.AddAsync(audit, cancellationToken);
    }
}
