using System;
using ERP.Modules.Integrations.Contracts;
using ERP.Modules.Inventory.Application.Services;
using ERP.Modules.Inventory.Domain;
using ERP.Modules.Inventory.Infrastructure.Repositories;
using ERP.Shared.Application;
using Xunit;

namespace ERP.Modules.Inventory.WhiteBox.Tests;

public sealed class InventoryFlowMovementTests
{
    [Fact]
    public async Task FullInventoryFlow_RecordsMovementsAndStock()
    {
        const long companyId = 1;
        const long supplierId = 77;
        const long customerId = 88;
        const long receiptId = 9001;
        const long shipmentIdWarehouse20 = 9101;
        const long shipmentIdWarehouse10 = 9102;
        const long productWidget = 100;
        const long productGadget = 200;
        const long warehouseMain = 10;
        const long warehouseOverflow = 20;

        var movementRepository = new InMemoryInventoryMovementRepository();
        var stockRepository = new InMemoryStockRepository();
        var outboxRepository = new InMemoryOutboxRepository();
        var eventPublisher = new CapturingEventPublisher();
        var inventoryFlowService = new InventoryFlowService(
            movementRepository,
            stockRepository,
            outboxRepository,
            eventPublisher);

        var receiptResult = await inventoryFlowService.ApplyGoodsReceiptAsync(
            companyId,
            receiptId,
            supplierId,
            purchaseOrderId: 5001,
            new[]
            {
                new GoodsReceiptLineInfo(productWidget, warehouseMain, 50m, "LOT-AX1", DateTime.UtcNow.Date.AddMonths(6)),
                new GoodsReceiptLineInfo(productGadget, warehouseMain, 30m, "LOT-BY2", DateTime.UtcNow.Date.AddMonths(4))
            });

        Assert.True(receiptResult.Success, receiptResult.Error);

        var transferResult = await inventoryFlowService.TransferStockAsync(
            companyId,
            productWidget,
            warehouseMain,
            warehouseOverflow,
            15m,
            referenceType: "Transfer",
            referenceId: "TR-001",
            reason: "Rebalance");

        Assert.True(transferResult.Success, transferResult.Error);

        var shipmentWarehouse20Result = await inventoryFlowService.ApplySalesShipmentAsync(
            companyId,
            shipmentIdWarehouse20,
            customerId,
            warehouseOverflow,
            new[] { new SalesShipmentLineInfo(productWidget, 5m, Array.Empty<StockMovementBatchInput>()) });

        Assert.True(shipmentWarehouse20Result.Success, shipmentWarehouse20Result.Error);

        var shipmentWarehouse10Result = await inventoryFlowService.ApplySalesShipmentAsync(
            companyId,
            shipmentIdWarehouse10,
            customerId,
            warehouseMain,
            new[] { new SalesShipmentLineInfo(productGadget, 10m, Array.Empty<StockMovementBatchInput>()) });

        Assert.True(shipmentWarehouse10Result.Success, shipmentWarehouse10Result.Error);

        var adjustmentResult = await inventoryFlowService.AdjustStockAsync(
            companyId,
            productGadget,
            warehouseMain,
            2m,
            increase: false,
            referenceType: "Damage",
            referenceId: "ADJ-001",
            reason: "Transit damage");

        Assert.True(adjustmentResult.Success, adjustmentResult.Error);

        var widgetMainStock = await stockRepository.GetAsync(companyId, productWidget, warehouseMain);
        var widgetOverflowStock = await stockRepository.GetAsync(companyId, productWidget, warehouseOverflow);
        var gadgetMainStock = await stockRepository.GetAsync(companyId, productGadget, warehouseMain);

        Assert.Equal(35m, widgetMainStock!.OnHandQuantity);
        Assert.Equal(10m, widgetOverflowStock!.OnHandQuantity);
        Assert.Equal(18m, gadgetMainStock!.OnHandQuantity);

        var receiptMovements = await movementRepository.ListByReferenceAsync(companyId, "GoodsReceipt", receiptId.ToString());
        var transferMovements = await movementRepository.ListByReferenceAsync(companyId, "Transfer", "TR-001");
        var shipmentMovementsWarehouse20 = await movementRepository.ListByReferenceAsync(companyId, "SalesShipment", shipmentIdWarehouse20.ToString());
        var shipmentMovementsWarehouse10 = await movementRepository.ListByReferenceAsync(companyId, "SalesShipment", shipmentIdWarehouse10.ToString());
        var adjustmentMovements = await movementRepository.ListByReferenceAsync(companyId, "Damage", "ADJ-001");

        Assert.Equal(2, receiptMovements.Count);
        Assert.Single(transferMovements);
        Assert.Single(shipmentMovementsWarehouse20);
        Assert.Single(shipmentMovementsWarehouse10);
        Assert.Single(adjustmentMovements);

        Assert.Contains(outboxRepository.Messages, message => message.Type == "inventory.goods-received");
        Assert.Contains(outboxRepository.Messages, message => message.Type == "inventory.sale-shipped");

        Assert.Equal(6, eventPublisher.PublishedEvents.Count);
    }

    private sealed class CapturingEventPublisher : IEventPublisher
    {
        public List<object> PublishedEvents { get; } = new();

        public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        {
            PublishedEvents.Add(@event!);
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryOutboxRepository : IOutboxRepository
    {
        private readonly List<OutboxMessage> _messages = new();

        public IReadOnlyList<OutboxMessage> Messages => _messages;

        public Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            _messages.Add(message);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(
            int batchSize,
            DateTime utcNow,
            int maxAttempts,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<OutboxMessage>>(_messages.Take(batchSize).ToList());

        public Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
