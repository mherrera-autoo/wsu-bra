using System.Text.Json;
using ERP.Modules.Integrations.Contracts;
using ERP.Modules.Inventory.Application.Repositories;
using ERP.Modules.Inventory.Application.Services;
using ERP.Modules.Inventory.Domain;
using ERP.Modules.PharmaceuticalRegulatedInventory.Contracts;
using ERP.Shared.Application;

namespace ERP.Modules.Inventory.Application.Handlers;

public sealed class PharmacyBatchReceivedHandler : IOutboxMessageHandler
{
    private const string SourceName = "pharmaceutical";
    private const string ReferenceType = "PharmacyBatchReceived";
    private readonly IInventoryFlowService _inventoryFlowService;
    private readonly IInventoryMovementRepository _movementRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PharmacyBatchReceivedHandler(
        IInventoryFlowService inventoryFlowService,
        IInventoryMovementRepository movementRepository,
        IUnitOfWork unitOfWork)
    {
        _inventoryFlowService = inventoryFlowService;
        _movementRepository = movementRepository;
        _unitOfWork = unitOfWork;
    }

    public string MessageType => "pharmacy.batch.received";

    public async Task HandleAsync(string payloadJson, CancellationToken cancellationToken = default)
    {
        var batchReceived = JsonSerializer.Deserialize<BatchReceivedIntegrationEvent>(payloadJson);
        if (batchReceived is null)
        {
            throw new InvalidOperationException("Invalid pharmacy batch received payload.");
        }

        var sourceId = batchReceived.SourceBatchPublicId.ToString("D");
        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            if (await _movementRepository.ExistsBySourceAsync(batchReceived.CompanyId, SourceName, sourceId, token))
            {
                return Result.Ok();
            }

            return await _inventoryFlowService.RecordStockInAsync(
                batchReceived.CompanyId,
                batchReceived.ProductId,
                batchReceived.WarehouseId,
                batchReceived.Quantity,
                ReferenceType,
                sourceId,
                null,
                Array.Empty<StockMovementBatchInput>(),
                null,
                token,
                SourceName,
                sourceId);
        }, cancellationToken);

        if (!result.Success)
        {
            throw new InvalidOperationException(result.Error ?? "Failed to apply pharmacy batch received movement.");
        }
    }
}
