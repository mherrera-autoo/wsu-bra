using ERP.Shared.Domain;

namespace ERP.Modules.Wsu.Domain;

public sealed class OrderItemEpcAssignment : Entity
{
    public long OrderId { get; private set; }
    public long OrderItemId { get; private set; }
    public OrderItem OrderItem { get; private set; } = null!;
    public string Epc { get; private set; } = null!;
    public bool IsChecked { get; private set; }

    private OrderItemEpcAssignment() { }

    public static OrderItemEpcAssignment Create(long orderId, long orderItemId, string epc)
    {
        if (orderId <= 0) throw new ArgumentOutOfRangeException(nameof(orderId));
        if (orderItemId <= 0) throw new ArgumentOutOfRangeException(nameof(orderItemId));
        if (string.IsNullOrWhiteSpace(epc)) throw new ArgumentException("Epc is required.", nameof(epc));

        return new OrderItemEpcAssignment
        {
            OrderId = orderId,
            OrderItemId = orderItemId,
            Epc = epc.Trim(),
            IsChecked = false
        };
    }

    public void SetChecked(bool isChecked)
    {
        IsChecked = isChecked;
        UpdatedAt = DateTime.UtcNow;
    }
}
