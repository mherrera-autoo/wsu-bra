using ERP.Shared.Domain;

namespace ERP.Modules.Inventory.Domain;

/// <summary>
/// Fast projection: current stock per (Product, Warehouse).
/// Truth remains InventoryMovement history.
/// </summary>
public sealed class Stock : CompanyEntity
{
    public long ProductId { get; private set; }
    public long WarehouseId { get; private set; }
    public decimal OnHandQuantity { get; private set; }

    private Stock() { }

    public static Stock Create(long companyId, long productId, long warehouseId, decimal onHand = 0)
        => new() { CompanyId = companyId, ProductId = productId, WarehouseId = warehouseId, OnHandQuantity = onHand };

    public void Increase(decimal quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be > 0.");
        OnHandQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Decrease(decimal quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be > 0.");
        if (OnHandQuantity - quantity < 0) throw new InvalidOperationException("Insufficient stock.");

        OnHandQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}
