using ERP.Modules.Inventory.Application.Repositories;
using ERP.Modules.Inventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class InventoryMovementRepository : IInventoryMovementRepository
{
    private readonly ErpDbContext _dbContext;

    public InventoryMovementRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(InventoryMovement movement, CancellationToken cancellationToken = default)
    {
        await _dbContext.InventoryMovements.AddAsync(movement, cancellationToken);
    }

    public async Task<IReadOnlyList<InventoryMovement>> ListByReferenceAsync(long companyId, string referenceType, string referenceId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.InventoryMovements
            .Where(movement =>
                movement.CompanyId == companyId &&
                movement.ReferenceType == referenceType &&
                movement.ReferenceId == referenceId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsBySourceAsync(long companyId, string source, string sourceId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.InventoryMovements.AnyAsync(
            movement =>
                movement.CompanyId == companyId &&
                movement.Source == source &&
                movement.SourceId == sourceId,
            cancellationToken);
    }
}
