using System.Text.Json;
using ERP.Modules.Integrations.Contracts;
using ERP.Modules.Inventory.Application.Services;
using ERP.Modules.PharmaceuticalRegulatedInventory.Contracts;
using ERP.Shared.Application;

namespace ERP.Modules.Inventory.Application.Handlers;

public sealed class PharmaceuticalRegulatedInventoryRecallReportedHandler : IOutboxMessageHandler
{
    private readonly IInventoryFlowService _inventoryFlowService;
    private readonly IUnitOfWork _unitOfWork;

    public PharmaceuticalRegulatedInventoryRecallReportedHandler(IInventoryFlowService inventoryFlowService, IUnitOfWork unitOfWork)
    {
        _inventoryFlowService = inventoryFlowService;
        _unitOfWork = unitOfWork;
    }

    public string MessageType => "pharmacy.recall.reported";

    public async Task HandleAsync(string payloadJson, CancellationToken cancellationToken = default)
    {
        var recall = JsonSerializer.Deserialize<PharmaceuticalRegulatedInventoryRecallReported>(payloadJson);
        if (recall is null)
        {
            throw new InvalidOperationException("Invalid regulated inventory recall payload.");
        }

        if (recall.Quantity is null || recall.Quantity <= 0)
        {
            return;
        }

        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            return await _inventoryFlowService.AdjustStockAsync(
                recall.CompanyId,
                recall.ProductId,
                recall.WarehouseId,
                recall.Quantity.Value,
                false,
                "PharmaceuticalRegulatedInventoryRecall",
                recall.RecallId.ToString(),
                recall.Reason,
                token);
        }, cancellationToken);

        if (!result.Success)
        {
            throw new InvalidOperationException(result.Error ?? "Failed to apply regulated inventory recall adjustment.");
        }
    }
}
