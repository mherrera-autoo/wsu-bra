using ERP.Shared.Domain;

namespace ERP.Modules.Inventory.Domain;

/// <summary>
/// Truth history: stock changes are recorded as movements (never manual edits).
/// </summary>
public sealed class InventoryMovement : CompanyEntity
{
    public long ProductId { get; private set; }
    public long? FromWarehouseId { get; private set; }
    public long? ToWarehouseId { get; private set; }
    public decimal Quantity { get; private set; } // positive; MovementType defines direction
    public MovementType MovementType { get; private set; }

    public string ReferenceType { get; private set; } = null!;
    public string ReferenceId { get; private set; } = null!;
    public string? Source { get; private set; }
    public string? SourceId { get; private set; }
    public string? Reason { get; private set; }

    private InventoryMovement() { }

    public static InventoryMovement Create(
        long companyId,
        long productId,
        MovementType movementType,
        decimal quantity,
        long? fromWarehouseId,
        long? toWarehouseId,
        string referenceType,
        string referenceId,
        string? reason = null,
        string? source = null,
        string? sourceId = null)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be > 0.");
        if (movementType == MovementType.Transfer && (fromWarehouseId is null || toWarehouseId is null))
            throw new InvalidOperationException("Transfer requires both from and to warehouses.");
        if (movementType == MovementType.In && toWarehouseId is null)
            throw new InvalidOperationException("IN movement requires ToWarehouseId.");
        if (movementType == MovementType.Out && fromWarehouseId is null)
            throw new InvalidOperationException("OUT movement requires FromWarehouseId.");
        if (!string.IsNullOrWhiteSpace(source) && string.IsNullOrWhiteSpace(sourceId))
            throw new InvalidOperationException("SourceId is required when Source is provided.");

        return new InventoryMovement
        {
            CompanyId = companyId,
            ProductId = productId,
            MovementType = movementType,
            Quantity = quantity,
            FromWarehouseId = fromWarehouseId,
            ToWarehouseId = toWarehouseId,
            ReferenceType = referenceType.Trim(),
            ReferenceId = referenceId.Trim(),
            Source = string.IsNullOrWhiteSpace(source) ? null : source.Trim(),
            SourceId = string.IsNullOrWhiteSpace(sourceId) ? null : sourceId.Trim(),
            Reason = reason
        };
    }
}
