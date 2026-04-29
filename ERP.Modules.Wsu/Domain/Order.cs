using ERP.Shared.Domain;

namespace ERP.Modules.Wsu.Domain;

public sealed class Order : Entity
{
    public Guid CompanyPublicId { get; private set; }
    public Guid? WarehousePublicId { get; private set; }
    public OrderType OrderType { get; private set; }
    public string OrderNumber { get; private set; } = null!;
    public string? ExternalOrderNumber { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTimeOffset MovementDate { get; private set; }
    public Guid? CreatedByUserPublicId { get; private set; }
    public SourceType SourceType { get; private set; }
    public string? Notes { get; private set; }
    public bool IsEpcLoadConfirmed { get; private set; }
    public bool IsEpcSkuMatchCompleted { get; private set; }
    public bool IsMovementProgrammed { get; private set; }

    public ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();
    public ICollection<WsuOrderMovementOperator> MovementOperators { get; private set; } = new List<WsuOrderMovementOperator>();

    private Order() { }

    public static Order Create(
        Guid companyPublicId,
        Guid? warehousePublicId,
        OrderType orderType,
        string orderNumber,
        string? externalOrderNumber,
        OrderStatus status,
        DateTimeOffset movementDate,
        Guid? createdByUserPublicId,
        SourceType sourceType,
        string? notes)
    {
        if (companyPublicId == Guid.Empty) throw new ArgumentException("CompanyPublicId is required.", nameof(companyPublicId));
        if (string.IsNullOrWhiteSpace(orderNumber)) throw new ArgumentException("OrderNumber is required.", nameof(orderNumber));

        return new Order
        {
            CompanyPublicId = companyPublicId,
            WarehousePublicId = warehousePublicId,
            OrderType = orderType,
            OrderNumber = orderNumber.Trim(),
            ExternalOrderNumber = NormalizeNullable(externalOrderNumber),
            Status = status,
            MovementDate = movementDate,
            CreatedByUserPublicId = createdByUserPublicId,
            SourceType = sourceType,
            Notes = NormalizeNullable(notes),
            IsEpcLoadConfirmed = false,
            IsEpcSkuMatchCompleted = false,
            IsMovementProgrammed = false
        };
    }

    public void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStatus(OrderStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkEpcLoadConfirmed()
    {
        IsEpcLoadConfirmed = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ResetEpcLoadConfirmed()
    {
        IsEpcLoadConfirmed = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetEpcSkuMatchCompleted(bool isCompleted)
    {
        IsEpcSkuMatchCompleted = isCompleted;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkMovementProgrammed()
    {
        IsMovementProgrammed = true;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
