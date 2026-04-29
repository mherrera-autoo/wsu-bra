using ERP.Shared.Domain;

namespace ERP.Modules.Wsu.Domain;

public sealed class OrderItemConsumption : Entity
{
    public long OutOrderItemId { get; private set; }
    public OrderItem OutOrderItem { get; private set; } = null!;
    public long InOrderItemId { get; private set; }
    public OrderItem InOrderItem { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal TotalCost { get; private set; }

    private OrderItemConsumption() { }

    public static OrderItemConsumption Create(
        long outOrderItemId,
        long inOrderItemId,
        decimal quantity,
        decimal unitCost)
    {
        if (outOrderItemId <= 0) throw new ArgumentOutOfRangeException(nameof(outOrderItemId));
        if (inOrderItemId <= 0) throw new ArgumentOutOfRangeException(nameof(inOrderItemId));
        if (quantity <= 0m) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (unitCost < 0m) throw new ArgumentOutOfRangeException(nameof(unitCost));

        return new OrderItemConsumption
        {
            OutOrderItemId = outOrderItemId,
            InOrderItemId = inOrderItemId,
            Quantity = quantity,
            UnitCost = unitCost,
            TotalCost = quantity * unitCost
        };
    }
}
