using ERP.Modules.Inventory.Application.Commands;
using ERP.Modules.Inventory.Application.Repositories;
using ERP.Modules.Inventory.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.Inventory.Application.Handlers;

public sealed class RecordInventoryMovementHandler
{
    private readonly IInventoryMovementRepository _movementRepository;
    private readonly IStockRepository _stockRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public RecordInventoryMovementHandler(
        IInventoryMovementRepository movementRepository,
        IStockRepository stockRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher)
    {
        _movementRepository = movementRepository;
        _stockRepository = stockRepository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result> HandleAsync(RecordInventoryMovementCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var movement = InventoryMovement.Create(
                command.CompanyId,
                command.ProductId,
                command.MovementType,
                command.Quantity,
                command.FromWarehouseId,
                command.ToWarehouseId,
                command.ReferenceType,
                command.ReferenceId,
                command.Reason);

            await ApplyStockProjectionAsync(movement, cancellationToken);
            await _movementRepository.AddAsync(movement, cancellationToken);
            await _eventPublisher.PublishAsync(new StockMoved(movement, Array.Empty<StockMovementBatchInput>(), null), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentOutOfRangeException)
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
}
