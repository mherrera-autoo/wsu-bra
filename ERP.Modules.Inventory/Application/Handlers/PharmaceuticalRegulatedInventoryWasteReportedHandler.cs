using System.Text.Json;
using ERP.Modules.Integrations.Contracts;
using ERP.Modules.Inventory.Application.Services;
using ERP.Modules.PharmaceuticalRegulatedInventory.Contracts;
using ERP.Shared.Application;

namespace ERP.Modules.Inventory.Application.Handlers;

public sealed class PharmaceuticalRegulatedInventoryWasteReportedHandler : IOutboxMessageHandler
{
    private readonly IInventoryFlowService _inventoryFlowService;
    private readonly IUnitOfWork _unitOfWork;

    public PharmaceuticalRegulatedInventoryWasteReportedHandler(IInventoryFlowService inventoryFlowService, IUnitOfWork unitOfWork)
    {
        _inventoryFlowService = inventoryFlowService;
        _unitOfWork = unitOfWork;
    }

    public string MessageType => "pharmacy.waste.reported";

    public async Task HandleAsync(string payloadJson, CancellationToken cancellationToken = default)
    {
        var waste = JsonSerializer.Deserialize<PharmaceuticalRegulatedInventoryWasteReported>(payloadJson);
        if (waste is null)
        {
            throw new InvalidOperationException("Invalid regulated inventory waste payload.");
        }

        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            return await _inventoryFlowService.AdjustStockAsync(
                waste.CompanyId,
                waste.ProductId,
                waste.WarehouseId,
                waste.Quantity,
                false,
                "PharmaceuticalRegulatedInventoryWaste",
                waste.WasteId.ToString(),
                waste.Reason,
                token);
        }, cancellationToken);

        if (!result.Success)
        {
            throw new InvalidOperationException(result.Error ?? "Failed to apply regulated inventory waste adjustment.");
        }
    }
}
