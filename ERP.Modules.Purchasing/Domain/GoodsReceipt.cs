using ERP.Shared.Domain;

namespace ERP.Modules.Purchasing.Domain;

/// <summary>
/// Receiving creates stock. This operational document triggers IN movements.
/// </summary>
public sealed class GoodsReceipt : CompanyEntity
{
    public long SupplierId { get; private set; }
    public long? PurchaseOrderId { get; private set; }
    public DateTime ReceivedAt { get; private set; } = DateTime.UtcNow;
    public List<GoodsReceiptLine> Lines { get; private set; } = new();

    private GoodsReceipt() { }

    public static GoodsReceipt Create(long companyId, long supplierId, long? purchaseOrderId = null)
        => new() { CompanyId = companyId, SupplierId = supplierId, PurchaseOrderId = purchaseOrderId };

    public void AddLine(long productId, long warehouseId, decimal receivedQty)
    {
        if (receivedQty <= 0) throw new ArgumentOutOfRangeException(nameof(receivedQty));
        Lines.Add(GoodsReceiptLine.Create(CompanyId, Id, productId, warehouseId, receivedQty));
    }
}

public sealed class GoodsReceiptLine : CompanyEntity
{
    public long GoodsReceiptId { get; private set; }
    public long ProductId { get; private set; }
    public long WarehouseId { get; private set; }
    public decimal ReceivedQty { get; private set; }

    private GoodsReceiptLine() { }

    public static GoodsReceiptLine Create(long companyId, long grId, long productId, long warehouseId, decimal receivedQty)
        => new() { CompanyId = companyId, GoodsReceiptId = grId, ProductId = productId, WarehouseId = warehouseId, ReceivedQty = receivedQty };
}
