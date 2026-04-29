using ERP.Shared.Domain;

namespace ERP.Modules.Wsu.Domain;

public sealed class WsuInventoryMovement : Entity
{
    public Guid CompanyPublicId { get; private set; }
    public Guid? WarehousePublicId { get; private set; }
    public long OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    public long OrderItemId { get; private set; }
    public OrderItem OrderItem { get; private set; } = null!;
    public Guid? ProductPublicId { get; private set; }
    public WsuMovementType MovementType { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal SignedQuantity { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public Guid? PerformedByUserPublicId { get; private set; }
    public SourceType SourceType { get; private set; }
    public string? Notes { get; private set; }

    public ICollection<WsuInventoryMovementOperator> Operators { get; private set; } = new List<WsuInventoryMovementOperator>();

    private WsuInventoryMovement() { }

    public static WsuInventoryMovement Create(
        Guid companyPublicId,
        Guid? warehousePublicId,
        long orderId,
        long orderItemId,
        Guid? productPublicId,
        WsuMovementType movementType,
        decimal quantity,
        decimal signedQuantity,
        DateTimeOffset occurredAt,
        Guid? performedByUserPublicId,
        SourceType sourceType,
        string? notes)
    {
        if (companyPublicId == Guid.Empty) throw new ArgumentException("CompanyPublicId is required.", nameof(companyPublicId));
        if (orderId <= 0) throw new ArgumentOutOfRangeException(nameof(orderId));
        if (orderItemId <= 0) throw new ArgumentOutOfRangeException(nameof(orderItemId));
        if (quantity <= 0m) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (signedQuantity == 0m) throw new ArgumentOutOfRangeException(nameof(signedQuantity));

        return new WsuInventoryMovement
        {
            CompanyPublicId = companyPublicId,
            WarehousePublicId = warehousePublicId,
            OrderId = orderId,
            OrderItemId = orderItemId,
            ProductPublicId = productPublicId,
            MovementType = movementType,
            Quantity = quantity,
            SignedQuantity = signedQuantity,
            OccurredAt = occurredAt,
            PerformedByUserPublicId = performedByUserPublicId,
            SourceType = sourceType,
            Notes = NormalizeNullable(notes)
        };
    }

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
