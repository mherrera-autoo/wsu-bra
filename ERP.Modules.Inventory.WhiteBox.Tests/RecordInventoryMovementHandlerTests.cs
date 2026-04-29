using ERP.Modules.Inventory.Application.Commands;
using ERP.Modules.Inventory.Application.Handlers;
using ERP.Modules.Inventory.Domain;
using ERP.Modules.Inventory.Infrastructure.Repositories;
using ERP.Shared.Application;
using Xunit;

namespace ERP.Modules.Inventory.WhiteBox.Tests;

public sealed class RecordInventoryMovementHandlerTests
{
    [Fact]
    public async Task HandleAsync_InMovement_IncreasesStock()
    {
        var stockRepository = new InMemoryStockRepository();
        var handler = BuildHandler(stockRepository);

        var result = await handler.HandleAsync(new RecordInventoryMovementCommand(
            CompanyId: 1,
            ProductId: 10,
            MovementType: MovementType.In,
            Quantity: 5,
            FromWarehouseId: null,
            ToWarehouseId: 100,
            ReferenceType: "PO",
            ReferenceId: "PO-001"));

        Assert.True(result.Success);

        var stock = await stockRepository.GetAsync(1, 10, 100);
        Assert.NotNull(stock);
        Assert.Equal(5m, stock!.OnHandQuantity);
    }

    [Fact]
    public async Task HandleAsync_OutMovement_InsufficientStock_Fails()
    {
        var stockRepository = new InMemoryStockRepository();
        await stockRepository.UpsertAsync(Stock.Create(1, 10, 200, onHand: 1));
        var handler = BuildHandler(stockRepository);

        var result = await handler.HandleAsync(new RecordInventoryMovementCommand(
            CompanyId: 1,
            ProductId: 10,
            MovementType: MovementType.Out,
            Quantity: 2,
            FromWarehouseId: 200,
            ToWarehouseId: null,
            ReferenceType: "SO",
            ReferenceId: "SO-001"));

        Assert.False(result.Success);
        Assert.Equal("Insufficient stock.", result.Error);
    }

    [Fact]
    public async Task HandleAsync_TransferMovement_UpdatesBothWarehouses()
    {
        var stockRepository = new InMemoryStockRepository();
        await stockRepository.UpsertAsync(Stock.Create(1, 10, 300, onHand: 5));
        var handler = BuildHandler(stockRepository);

        var result = await handler.HandleAsync(new RecordInventoryMovementCommand(
            CompanyId: 1,
            ProductId: 10,
            MovementType: MovementType.Transfer,
            Quantity: 2,
            FromWarehouseId: 300,
            ToWarehouseId: 400,
            ReferenceType: "TR",
            ReferenceId: "TR-001"));

        Assert.True(result.Success);

        var fromStock = await stockRepository.GetAsync(1, 10, 300);
        var toStock = await stockRepository.GetAsync(1, 10, 400);

        Assert.Equal(3m, fromStock!.OnHandQuantity);
        Assert.Equal(2m, toStock!.OnHandQuantity);
    }

    [Fact]
    public async Task HandleAsync_AdjustmentWithBothWarehouses_Fails()
    {
        var stockRepository = new InMemoryStockRepository();
        var handler = BuildHandler(stockRepository);

        var result = await handler.HandleAsync(new RecordInventoryMovementCommand(
            CompanyId: 1,
            ProductId: 10,
            MovementType: MovementType.Adjustment,
            Quantity: 1,
            FromWarehouseId: 500,
            ToWarehouseId: 600,
            ReferenceType: "ADJ",
            ReferenceId: "ADJ-001"));

        Assert.False(result.Success);
        Assert.Equal("Adjustment movement must target a single warehouse.", result.Error);
    }

    private static RecordInventoryMovementHandler BuildHandler(InMemoryStockRepository stockRepository)
    {
        return new RecordInventoryMovementHandler(
            new InMemoryInventoryMovementRepository(),
            stockRepository,
            new InMemoryUnitOfWork(),
            new NoopEventPublisher());
    }

    private sealed class NoopEventPublisher : IEventPublisher
    {
        public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
