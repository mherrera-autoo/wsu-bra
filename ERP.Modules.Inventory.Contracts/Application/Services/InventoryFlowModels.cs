using ERP.Modules.Inventory.Domain;

namespace ERP.Modules.Inventory.Application.Services;

public sealed record GoodsReceiptLineInfo(long ProductId, long WarehouseId, decimal ReceivedQty, string? BatchNumber, DateTime? ExpiryDate);

public sealed record SalesShipmentLineInfo(long ProductId, decimal Qty, IReadOnlyList<StockMovementBatchInput>? BatchInputs);
