using ERP.Modules.Inventory.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.Inventory.Application.Services;

public interface IInventoryFlowService
{
    Task<Result> ApplyGoodsReceiptAsync(
        long companyId,
        long receiptId,
        long supplierId,
        long? purchaseOrderId,
        IEnumerable<GoodsReceiptLineInfo> lines,
        CancellationToken cancellationToken = default);

    Task<Result> ApplySalesShipmentAsync(
        long companyId,
        long documentId,
        long customerId,
        long warehouseId,
        IEnumerable<SalesShipmentLineInfo> lines,
        CancellationToken cancellationToken = default);

    Task<Result> TransferStockAsync(
        long companyId,
        long productId,
        long fromWarehouseId,
        long toWarehouseId,
        decimal quantity,
        string referenceType,
        string referenceId,
        string? reason,
        CancellationToken cancellationToken = default);

    Task<Result> RecordStockOutAsync(
        long companyId,
        long productId,
        long warehouseId,
        decimal quantity,
        string referenceType,
        string referenceId,
        string? reason,
        IReadOnlyList<StockMovementBatchInput> batchInputs,
        StockMovementReference? traceReference = null,
        CancellationToken cancellationToken = default);

    Task<Result> RecordStockInAsync(
        long companyId,
        long productId,
        long warehouseId,
        decimal quantity,
        string referenceType,
        string referenceId,
        string? reason,
        IReadOnlyList<StockMovementBatchInput> batchInputs,
        StockMovementReference? traceReference = null,
        CancellationToken cancellationToken = default,
        string? source = null,
        string? sourceId = null);

    Task<Result> AdjustStockAsync(
        long companyId,
        long productId,
        long warehouseId,
        decimal quantity,
        bool increase,
        string referenceType,
        string referenceId,
        string? reason,
        CancellationToken cancellationToken = default);
}
