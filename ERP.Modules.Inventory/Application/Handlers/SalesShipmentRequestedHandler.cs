using System;
using System.Text.Json;
using ERP.Modules.Integrations.Contracts;
using ERP.Modules.Inventory.Application.Services;
using ERP.Modules.Inventory.Domain;
using ERP.Modules.Sales.Contracts;
using ERP.Shared.Application;

namespace ERP.Modules.Inventory.Application.Handlers;

public sealed class SalesShipmentRequestedHandler : IOutboxMessageHandler
{
    private readonly IInventoryFlowService _inventoryFlowService;
    private readonly IUnitOfWork _unitOfWork;

    public SalesShipmentRequestedHandler(IInventoryFlowService inventoryFlowService, IUnitOfWork unitOfWork)
    {
        _inventoryFlowService = inventoryFlowService;
        _unitOfWork = unitOfWork;
    }

    public string MessageType => "sales.shipment.requested";

    public async Task HandleAsync(string payloadJson, CancellationToken cancellationToken = default)
    {
        var shipment = JsonSerializer.Deserialize<SalesShipmentRequested>(payloadJson);
        if (shipment is null)
        {
            throw new InvalidOperationException("Invalid sales shipment payload.");
        }

        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var lines = shipment.Lines.Select(line => new SalesShipmentLineInfo(
                line.ProductId,
                line.Quantity,
                line.BatchInputs is null
                    ? Array.Empty<StockMovementBatchInput>()
                    : line.BatchInputs
                        .Select(input => new StockMovementBatchInput(input.BatchNumber, input.ExpiryDate, input.Quantity))
                        .ToList()));
            return await _inventoryFlowService.ApplySalesShipmentAsync(
                shipment.CompanyId,
                shipment.DocumentId,
                shipment.CustomerId,
                shipment.WarehouseId,
                lines,
                token);
        }, cancellationToken);

        if (!result.Success)
        {
            throw new InvalidOperationException(result.Error ?? "Failed to apply sales shipment.");
        }
    }
}
