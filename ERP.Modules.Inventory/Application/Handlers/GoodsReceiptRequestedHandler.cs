using System.Text.Json;
using ERP.Modules.Integrations.Contracts;
using ERP.Modules.Inventory.Application.Services;
using ERP.Modules.Purchasing.Contracts;
using ERP.Shared.Application;

namespace ERP.Modules.Inventory.Application.Handlers;

public sealed class GoodsReceiptRequestedHandler : IOutboxMessageHandler
{
    private readonly IInventoryFlowService _inventoryFlowService;
    private readonly IUnitOfWork _unitOfWork;

    public GoodsReceiptRequestedHandler(IInventoryFlowService inventoryFlowService, IUnitOfWork unitOfWork)
    {
        _inventoryFlowService = inventoryFlowService;
        _unitOfWork = unitOfWork;
    }

    public string MessageType => "purchasing.goods-receipt.requested";

    public async Task HandleAsync(string payloadJson, CancellationToken cancellationToken = default)
    {
        var receipt = JsonSerializer.Deserialize<GoodsReceiptRequested>(payloadJson);
        if (receipt is null)
        {
            throw new InvalidOperationException("Invalid goods receipt payload.");
        }

        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var lines = receipt.Lines.Select(line => new GoodsReceiptLineInfo(
                line.ProductId,
                line.WarehouseId,
                line.ReceivedQty,
                line.BatchNumber,
                line.ExpiryDate));

            return await _inventoryFlowService.ApplyGoodsReceiptAsync(
                receipt.CompanyId,
                receipt.ReceiptId,
                receipt.SupplierId,
                receipt.PurchaseOrderId,
                lines,
                token);
        }, cancellationToken);

        if (!result.Success)
        {
            throw new InvalidOperationException(result.Error ?? "Failed to apply goods receipt.");
        }
    }
}
