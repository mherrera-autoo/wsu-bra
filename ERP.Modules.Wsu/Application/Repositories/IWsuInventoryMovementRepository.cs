using ERP.Modules.Wsu.Domain;

namespace ERP.Modules.Wsu.Application.Repositories;

public interface IWsuInventoryMovementRepository
{
    Task AddAsync(WsuInventoryMovement item, CancellationToken cancellationToken = default);
}
