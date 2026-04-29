using ERP.Modules.Inventory.Application.Repositories;
using ERP.Modules.Inventory.Domain;

namespace ERP.Modules.Inventory.Infrastructure.Repositories;

public sealed class InMemoryInventoryMovementRepository : IInventoryMovementRepository
{
    private readonly List<InventoryMovement> _movements = new();

    public Task AddAsync(InventoryMovement movement, CancellationToken cancellationToken = default)
    {
        if (movement is null) throw new ArgumentNullException(nameof(movement));
        _movements.Add(movement);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<InventoryMovement>> ListByReferenceAsync(long companyId, string referenceType, string referenceId, CancellationToken cancellationToken = default)
    {
        var results = _movements
            .Where(movement =>
                movement.CompanyId == companyId &&
                movement.ReferenceType == referenceType &&
                movement.ReferenceId == referenceId)
            .ToList()
            .AsReadOnly();
        return Task.FromResult<IReadOnlyList<InventoryMovement>>(results);
    }

    public Task<bool> ExistsBySourceAsync(long companyId, string source, string sourceId, CancellationToken cancellationToken = default)
    {
        var exists = _movements.Any(movement =>
            movement.CompanyId == companyId &&
            movement.Source == source &&
            movement.SourceId == sourceId);
        return Task.FromResult(exists);
    }
}
