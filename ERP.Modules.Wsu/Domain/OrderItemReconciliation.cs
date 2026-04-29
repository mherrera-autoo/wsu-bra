using ERP.Shared.Domain;

namespace ERP.Modules.Wsu.Domain;

public sealed class OrderItemReconciliation : Entity
{
    public long OrderId { get; private set; }
    public long OrderItemId { get; private set; }
    public OrderItem OrderItem { get; private set; } = null!;
    public decimal ReconciledQuantity { get; private set; }
    public DateTimeOffset ReconciledAt { get; private set; }
    public Guid? ReconciledByUserPublicId { get; private set; }
    public SourceType SourceType { get; private set; }
    public string? Notes { get; private set; }

    private OrderItemReconciliation() { }

    public static OrderItemReconciliation Create(
        long orderId,
        long orderItemId,
        decimal reconciledQuantity,
        DateTimeOffset reconciledAt,
        Guid? reconciledByUserPublicId,
        SourceType sourceType,
        string? notes)
    {
        if (orderId <= 0) throw new ArgumentOutOfRangeException(nameof(orderId));
        if (orderItemId <= 0) throw new ArgumentOutOfRangeException(nameof(orderItemId));
        if (reconciledQuantity <= 0m) throw new ArgumentOutOfRangeException(nameof(reconciledQuantity));

        return new OrderItemReconciliation
        {
            OrderId = orderId,
            OrderItemId = orderItemId,
            ReconciledQuantity = reconciledQuantity,
            ReconciledAt = reconciledAt,
            ReconciledByUserPublicId = reconciledByUserPublicId,
            SourceType = sourceType,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        };
    }
}
