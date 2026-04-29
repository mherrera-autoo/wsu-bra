using System.Text.Json;
using ERP.Modules.Integrations.Contracts;
using ERP.Modules.Inventory.Application.Repositories;
using ERP.Modules.Inventory.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.Inventory.Application.Services;

public sealed class InventoryFlowService : IInventoryFlowService
{
    private readonly IInventoryMovementRepository _movementRepository;
    private readonly IStockRepository _stockRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IEventPublisher _eventPublisher;

    public InventoryFlowService(
        IInventoryMovementRepository movementRepository,
        IStockRepository stockRepository,
        IOutboxRepository outboxRepository,
        IEventPublisher eventPublisher)
    {
        _movementRepository = movementRepository;
        _stockRepository = stockRepository;
        _outboxRepository = outboxRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result> ApplyGoodsReceiptAsync(
        long companyId,
        long receiptId,
        long supplierId,
        long? purchaseOrderId,
        IEnumerable<GoodsReceiptLineInfo> lines,
        CancellationToken cancellationToken = default)
    {
        foreach (var line in lines)
        {
            var batchInputs = BuildBatchInputs(line.BatchNumber, line.ExpiryDate, line.ReceivedQty);
            var movementResult = await RecordMovementAsync(
                companyId,
                line.ProductId,
                MovementType.In,
                line.ReceivedQty,
                null,
                line.WarehouseId,
                "GoodsReceipt",
                receiptId.ToString(),
                null,
                batchInputs,
                new StockMovementReference(supplierId, null, null),
                cancellationToken);

            if (!movementResult.Success)
            {
                return movementResult;
            }
        }

        var payload = JsonSerializer.Serialize(new
        {
            receiptId,
            companyId,
            supplierId,
            purchaseOrderId,
            lines = lines.Select(l => new { l.ProductId, l.WarehouseId, l.ReceivedQty })
        });

        await _outboxRepository.AddAsync(OutboxMessage.Create("inventory.goods-received", payload), cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> ApplySalesShipmentAsync(
        long companyId,
        long documentId,
        long customerId,
        long warehouseId,
        IEnumerable<SalesShipmentLineInfo> lines,
        CancellationToken cancellationToken = default)
    {
        foreach (var line in lines)
        {
            var batchInputs = line.BatchInputs ?? Array.Empty<StockMovementBatchInput>();
            var movementResult = await RecordMovementAsync(
                companyId,
                line.ProductId,
                MovementType.Out,
                line.Qty,
                warehouseId,
                null,
                "SalesShipment",
                documentId.ToString(),
                null,
                batchInputs,
                new StockMovementReference(null, customerId, null),
                cancellationToken);

            if (!movementResult.Success)
            {
                return movementResult;
            }
        }

        var payload = JsonSerializer.Serialize(new
        {
            documentId,
            companyId,
            customerId,
            warehouseId,
            lines = lines.Select(l => new { l.ProductId, l.Qty })
        });

        await _outboxRepository.AddAsync(OutboxMessage.Create("inventory.sale-shipped", payload), cancellationToken);
        return Result.Ok();
    }

    public Task<Result> TransferStockAsync(
        long companyId,
        long productId,
        long fromWarehouseId,
        long toWarehouseId,
        decimal quantity,
        string referenceType,
        string referenceId,
        string? reason,
        CancellationToken cancellationToken = default)
        => RecordMovementAsync(companyId, productId, MovementType.Transfer, quantity, fromWarehouseId, toWarehouseId, referenceType, referenceId, reason, Array.Empty<StockMovementBatchInput>(), null, cancellationToken);

    public Task<Result> RecordStockOutAsync(
        long companyId,
        long productId,
        long warehouseId,
        decimal quantity,
        string referenceType,
        string referenceId,
        string? reason,
        IReadOnlyList<StockMovementBatchInput> batchInputs,
        StockMovementReference? traceReference = null,
        CancellationToken cancellationToken = default)
        => RecordMovementAsync(companyId, productId, MovementType.Out, quantity, warehouseId, null, referenceType, referenceId, reason, batchInputs, traceReference, cancellationToken);

    public Task<Result> RecordStockInAsync(
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
        string? sourceId = null)
        => RecordMovementAsync(companyId, productId, MovementType.In, quantity, null, warehouseId, referenceType, referenceId, reason, batchInputs, traceReference, cancellationToken, source, sourceId);

    public Task<Result> AdjustStockAsync(
        long companyId,
        long productId,
        long warehouseId,
        decimal quantity,
        bool increase,
        string referenceType,
        string referenceId,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        return increase
            ? RecordMovementAsync(companyId, productId, MovementType.Adjustment, quantity, null, warehouseId, referenceType, referenceId, reason, Array.Empty<StockMovementBatchInput>(), null, cancellationToken)
            : RecordMovementAsync(companyId, productId, MovementType.Adjustment, quantity, warehouseId, null, referenceType, referenceId, reason, Array.Empty<StockMovementBatchInput>(), null, cancellationToken);
    }

    private async Task<Result> RecordMovementAsync(
        long companyId,
        long productId,
        MovementType movementType,
        decimal quantity,
        long? fromWarehouseId,
        long? toWarehouseId,
        string referenceType,
        string referenceId,
        string? reason,
        IReadOnlyList<StockMovementBatchInput> batchInputs,
        StockMovementReference? traceReference,
        CancellationToken cancellationToken,
        string? source = null,
        string? sourceId = null)
    {
        try
        {
            var movement = InventoryMovement.Create(
                companyId,
                productId,
                movementType,
                quantity,
                fromWarehouseId,
                toWarehouseId,
                referenceType,
                referenceId,
                reason,
                source,
                sourceId);

            await ApplyStockProjectionAsync(movement, cancellationToken);
            await _movementRepository.AddAsync(movement, cancellationToken);
            await _eventPublisher.PublishAsync(new StockMoved(movement, batchInputs, traceReference), cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentOutOfRangeException)
        {
            return Result.Fail(ex.Message);
        }
    }

    private async Task ApplyStockProjectionAsync(InventoryMovement movement, CancellationToken cancellationToken)
    {
        switch (movement.MovementType)
        {
            case MovementType.In:
                await IncreaseAsync(movement.CompanyId, movement.ProductId, movement.ToWarehouseId!.Value, movement.Quantity, cancellationToken);
                break;
            case MovementType.Out:
                await DecreaseAsync(movement.CompanyId, movement.ProductId, movement.FromWarehouseId!.Value, movement.Quantity, cancellationToken);
                break;
            case MovementType.Transfer:
                await DecreaseAsync(movement.CompanyId, movement.ProductId, movement.FromWarehouseId!.Value, movement.Quantity, cancellationToken);
                await IncreaseAsync(movement.CompanyId, movement.ProductId, movement.ToWarehouseId!.Value, movement.Quantity, cancellationToken);
                break;
            case MovementType.Adjustment:
                await ApplyAdjustmentAsync(movement, cancellationToken);
                break;
            default:
                throw new InvalidOperationException($"Unsupported movement type: {movement.MovementType}.");
        }
    }

    private async Task ApplyAdjustmentAsync(InventoryMovement movement, CancellationToken cancellationToken)
    {
        if (movement.ToWarehouseId is not null && movement.FromWarehouseId is not null)
            throw new InvalidOperationException("Adjustment movement must target a single warehouse.");

        if (movement.ToWarehouseId is null && movement.FromWarehouseId is null)
            throw new InvalidOperationException("Adjustment movement requires a warehouse.");

        if (movement.ToWarehouseId is not null)
        {
            await IncreaseAsync(movement.CompanyId, movement.ProductId, movement.ToWarehouseId.Value, movement.Quantity, cancellationToken);
            return;
        }

        await DecreaseAsync(movement.CompanyId, movement.ProductId, movement.FromWarehouseId!.Value, movement.Quantity, cancellationToken);
    }

    private async Task IncreaseAsync(long companyId, long productId, long warehouseId, decimal quantity, CancellationToken cancellationToken)
    {
        var stock = await _stockRepository.GetAsync(companyId, productId, warehouseId, cancellationToken)
            ?? Stock.Create(companyId, productId, warehouseId);

        stock.Increase(quantity);
        await _stockRepository.UpsertAsync(stock, cancellationToken);
    }

    private async Task DecreaseAsync(long companyId, long productId, long warehouseId, decimal quantity, CancellationToken cancellationToken)
    {
        var stock = await _stockRepository.GetAsync(companyId, productId, warehouseId, cancellationToken)
            ?? Stock.Create(companyId, productId, warehouseId);

        stock.Decrease(quantity);
        await _stockRepository.UpsertAsync(stock, cancellationToken);
    }

    private static IReadOnlyList<StockMovementBatchInput> BuildBatchInputs(string? batchNumber, DateTime? expiryDate, decimal quantity)
    {
        if (string.IsNullOrWhiteSpace(batchNumber))
        {
            return Array.Empty<StockMovementBatchInput>();
        }

        return new[] { new StockMovementBatchInput(batchNumber.Trim(), expiryDate, quantity) };
    }
}
