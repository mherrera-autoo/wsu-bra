using ERP.Modules.Inventory.Domain;

namespace ERP.Modules.Inventory.Application.Repositories;

public interface IInventoryMovementRepository
{
    Task AddAsync(InventoryMovement movement, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryMovement>> ListByReferenceAsync(long companyId, string referenceType, string referenceId, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySourceAsync(long companyId, string source, string sourceId, CancellationToken cancellationToken = default);
}
