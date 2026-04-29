using ERP.Modules.Inventory.Domain;

namespace ERP.Modules.Inventory.Application.Commands;

public sealed record RecordInventoryMovementCommand(
    long CompanyId,
    long ProductId,
    MovementType MovementType,
    decimal Quantity,
    long? FromWarehouseId,
    long? ToWarehouseId,
    string ReferenceType,
    string ReferenceId,
    string? Reason = null);
