using System;
using ERP.Modules.Integrations.Contracts;
using ERP.Modules.Inventory.Application.Services;
using ERP.Modules.Inventory.Domain;
using ERP.Modules.Inventory.Infrastructure.Repositories;
using ERP.Shared.Application;
using Xunit;

namespace ERP.Modules.Inventory.WhiteBox.Tests;

public sealed class InventoryFlowServiceTests
{
    [Fact]
    public async Task ApplyReceiptAndShipment_AdjustsStock_AndEnqueuesOutboxMessages()
    {
        const long companyId = 1;
        const long supplierId = 55;
        const long customerId = 88;
        const long receiptId = 901;
        const long shipmentId = 902;
        const long productId = 100;
        const long warehouseId = 10;

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
                new GoodsReceiptLineInfo(productId, warehouseId, 12m, "LOT-001", DateTime.UtcNow.Date.AddMonths(6))
            });

        Assert.True(receiptResult.Success, receiptResult.Error);

        var shipmentResult = await inventoryFlowService.ApplySalesShipmentAsync(
            companyId,
            shipmentId,
            customerId,
            warehouseId,
            new[]
            {
                new SalesShipmentLineInfo(productId, 5m, Array.Empty<StockMovementBatchInput>())
            });

        Assert.True(shipmentResult.Success, shipmentResult.Error);

        var stock = await stockRepository.GetAsync(companyId, productId, warehouseId);
        Assert.NotNull(stock);
        Assert.Equal(7m, stock!.OnHandQuantity);

        var outboxTypes = outboxRepository.Messages.Select(message => message.Type).ToList();
        Assert.Contains("inventory.goods-received", outboxTypes);
        Assert.Contains("inventory.sale-shipped", outboxTypes);

        Assert.Equal(2, eventPublisher.PublishedEvents.Count);
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
