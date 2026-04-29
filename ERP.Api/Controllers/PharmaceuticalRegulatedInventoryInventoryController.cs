using System.Text.Json;
using System;
using System.Linq;
using ERP.Api.Authorization;
using ERP.Api.Contracts.PharmaceuticalRegulatedInventory;
using ERP.Api.Filters;
using ERP.Modules.Accounting.Application.Services;
using ERP.Modules.Integrations.Contracts;
using ERP.Modules.Inventory.Application.Services;
using ERP.Modules.Inventory.Domain;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Services;
using ERP.Modules.PharmaceuticalRegulatedInventory.Contracts;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using ERP.Modules.Wms.Application.Repositories;
using ERP.Modules.Purchasing.Application.Services;
using ERP.Shared.Application;
using ERP.Shared.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[RequireFeature(PharmacyFeatureKeys.Base)]
[Route("api/pharmaceutical-regulated-inventory/inventory")]
public sealed class PharmaceuticalRegulatedInventoryInventoryController : ControllerBase
{
    private readonly IStockBatchRepository _stockBatchRepository;
    private readonly IQuarantineHoldRepository _quarantineHoldRepository;
    private readonly IBlockHoldRepository _blockHoldRepository;
    private readonly IWarehouseLocationRepository _warehouseLocationRepository;
    private readonly IPurchaseInvoiceIngestionRepository _ingestionRepository;
    private readonly IProductPharmaInfoRepository _productPharmaInfoRepository;
    private readonly IProductBarcodeLookup _productBarcodeLookup;
    private readonly IRecallRepository _recallRepository;
    private readonly IWasteRepository _wasteRepository;
    private readonly IPharmacyCriticalAuditRepository _criticalAuditRepository;
    private readonly IPharmacyTraceEventRepository _traceEventRepository;
    private readonly IInventoryFlowService _inventoryFlowService;
    private readonly PurchasingService _purchasingService;
    private readonly PayablesService _payablesService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IPurchaseInvoiceQrParser _qrParser;
    private readonly ICurrentUserProvider _currentUserProvider;

    public PharmaceuticalRegulatedInventoryInventoryController(
        IStockBatchRepository stockBatchRepository,
        IQuarantineHoldRepository quarantineHoldRepository,
        IBlockHoldRepository blockHoldRepository,
        IWarehouseLocationRepository warehouseLocationRepository,
        IPurchaseInvoiceIngestionRepository ingestionRepository,
        IProductPharmaInfoRepository productPharmaInfoRepository,
        IProductBarcodeLookup productBarcodeLookup,
        IRecallRepository recallRepository,
        IWasteRepository wasteRepository,
        IPharmacyCriticalAuditRepository criticalAuditRepository,
        IPharmacyTraceEventRepository traceEventRepository,
        IInventoryFlowService inventoryFlowService,
        PurchasingService purchasingService,
        PayablesService payablesService,
        IUnitOfWork unitOfWork,
        IOutboxRepository outboxRepository,
        IPurchaseInvoiceQrParser qrParser,
        ICurrentUserProvider currentUserProvider)
    {
        _stockBatchRepository = stockBatchRepository;
        _quarantineHoldRepository = quarantineHoldRepository;
        _blockHoldRepository = blockHoldRepository;
        _warehouseLocationRepository = warehouseLocationRepository;
        _ingestionRepository = ingestionRepository;
        _productPharmaInfoRepository = productPharmaInfoRepository;
        _productBarcodeLookup = productBarcodeLookup;
        _recallRepository = recallRepository;
        _wasteRepository = wasteRepository;
        _criticalAuditRepository = criticalAuditRepository;
        _traceEventRepository = traceEventRepository;
        _inventoryFlowService = inventoryFlowService;
        _purchasingService = purchasingService;
        _payablesService = payablesService;
        _unitOfWork = unitOfWork;
        _outboxRepository = outboxRepository;
        _qrParser = qrParser;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet("batches")]
    [RequireCompanyPermission(PermissionKeys.Inventory.StockRead)]
    public async Task<IActionResult> GetBatches([FromQuery] long? productId, [FromQuery] long? warehouseId, [FromQuery] StockBatchHealthStatus? status, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var currentUser))
        {
            return Unauthorized();
        }

        var batches = await _stockBatchRepository.ListAsync(currentUser.CompanyId, productId, warehouseId, status, cancellationToken);
        var response = batches.Select(batch => new PharmaceuticalRegulatedInventoryBatchResponse(
            batch.Id,
            batch.ProductId,
            batch.WarehouseId,
            batch.WarehouseLocationId,
            batch.BatchNumber,
            batch.ExpiryDate,
            batch.QuantityOnHand,
            batch.HealthStatus));

        return Ok(response);
    }

    [HttpPost("assign-location")]
    [RequireCompanyPermission(PermissionKeys.Inventory.AdjustmentsCreate)]
    public async Task<IActionResult> AssignLocation(BatchAssignmentRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var currentUser))
        {
            return Unauthorized();
        }

        if (request.CompanyId != currentUser.CompanyId)
        {
            return Forbid();
        }

        var batch = await _stockBatchRepository.GetByIdAsync(request.StockBatchId, cancellationToken);
        if (batch is null || batch.CompanyId != currentUser.CompanyId)
        {
            return NotFound(new { error = "Stock batch not found." });
        }

        if (request.WarehouseLocationId.HasValue)
        {
            var location = await _warehouseLocationRepository.GetByIdAsync(currentUser.CompanyId, request.WarehouseLocationId.Value, cancellationToken);
            if (location is null || location.WarehouseId != batch.WarehouseId)
            {
                return BadRequest(new { error = "Warehouse location not found for this warehouse." });
            }
        }

        batch.AssignLocation(request.WarehouseLocationId);
        await _stockBatchRepository.UpdateAsync(batch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(new { status = "ok" });
    }

    [HttpPost("quarantine")]
    [RequireCompanyPermission(PermissionKeys.Inventory.AdjustmentsCreate)]
    public async Task<IActionResult> PlaceQuarantine(PlaceQuarantineRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var currentUser))
        {
            return Unauthorized();
        }

        if (request.CompanyId != currentUser.CompanyId)
        {
            return Forbid();
        }

        if (!CanManageHold(currentUser))
        {
            return Forbid();
        }

        if (request.Status is not (StockBatchHealthStatus.Quarantine or StockBatchHealthStatus.Hold))
        {
            return BadRequest(new { error = "Status must be Quarantine or Hold." });
        }

        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var batch = await _stockBatchRepository.GetByIdAsync(request.StockBatchId, token);
            if (batch is null || batch.CompanyId != currentUser.CompanyId)
            {
                return Result.Fail("Stock batch not found.");
            }

            if (batch.HealthStatus is StockBatchHealthStatus.Recall or StockBatchHealthStatus.Expired or StockBatchHealthStatus.Blocked)
            {
                return Result.Fail("Stock batch is not eligible for quarantine/hold.");
            }

            var existing = await _quarantineHoldRepository.GetActiveByBatchAsync(currentUser.CompanyId, batch.Id, token);
            if (existing is not null)
            {
                return Result.Fail("Stock batch already has an active quarantine/hold.");
            }

            var hold = QuarantineHold.Create(currentUser.CompanyId, batch.Id, request.Status, request.Reason, currentUser.UserId);
            batch.PlaceOnHold(request.Status);

            await _quarantineHoldRepository.AddAsync(hold, token);
            await _stockBatchRepository.UpdateAsync(batch, token);
            await RecordCriticalAuditAsync(
                batch.CompanyId,
                nameof(StockBatch),
                batch.Id.ToString(),
                "BatchHoldPlaced",
                new
                {
                    batch.Id,
                    batch.BatchNumber,
                    batch.ProductId,
                    batch.WarehouseId,
                    request.Status,
                    request.Reason,
                    batch.QuantityOnHand
                },
                currentUser.Email,
                token);
            return Result.Ok();
        }, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "ok" });
    }

    [HttpPost("holds")]
    [RequireCompanyPermission(PermissionKeys.Inventory.AdjustmentsCreate)]
    public async Task<IActionResult> PlaceHold(BatchHoldRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var currentUser))
        {
            return Unauthorized();
        }

        if (request.CompanyId != currentUser.CompanyId)
        {
            return Forbid();
        }

        if (!CanManageHold(currentUser))
        {
            return Forbid();
        }

        if (request.Status is not (StockBatchHealthStatus.Quarantine or StockBatchHealthStatus.Hold or StockBatchHealthStatus.Blocked))
        {
            return BadRequest(new { error = "Status must be Quarantine, Hold, or Blocked." });
        }

        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var batch = await _stockBatchRepository.GetByIdAsync(request.StockBatchId, token);
            if (batch is null || batch.CompanyId != currentUser.CompanyId)
            {
                return Result.Fail("Stock batch not found.");
            }

            if (batch.HealthStatus is StockBatchHealthStatus.Recall or StockBatchHealthStatus.Expired)
            {
                return Result.Fail("Stock batch is not eligible for hold.");
            }

            if (request.Status == StockBatchHealthStatus.Blocked)
            {
                var existingBlock = await _blockHoldRepository.GetActiveByBatchAsync(currentUser.CompanyId, batch.Id, token);
                if (existingBlock is not null)
                {
                    return Result.Fail("Stock batch already has an active block.");
                }

                var block = BlockHold.Create(currentUser.CompanyId, batch.Id, request.Reason, currentUser.UserId);
                batch.PlaceOnHold(StockBatchHealthStatus.Blocked);
                await _blockHoldRepository.AddAsync(block, token);
            }
            else
            {
                var existing = await _quarantineHoldRepository.GetActiveByBatchAsync(currentUser.CompanyId, batch.Id, token);
                if (existing is not null)
                {
                    return Result.Fail("Stock batch already has an active quarantine/hold.");
                }

                var hold = QuarantineHold.Create(currentUser.CompanyId, batch.Id, request.Status, request.Reason, currentUser.UserId);
                batch.PlaceOnHold(request.Status);
                await _quarantineHoldRepository.AddAsync(hold, token);
            }

            await _stockBatchRepository.UpdateAsync(batch, token);
            await RecordCriticalAuditAsync(
                batch.CompanyId,
                nameof(StockBatch),
                batch.Id.ToString(),
                "BatchHoldPlaced",
                new
                {
                    batch.Id,
                    batch.BatchNumber,
                    batch.ProductId,
                    batch.WarehouseId,
                    request.Status,
                    request.Reason,
                    batch.QuantityOnHand
                },
                currentUser.Email,
                token);

            var holdEvent = new PharmaceuticalRegulatedInventoryBatchHeld(
                batch.CompanyId,
                batch.Id,
                batch.BatchNumber,
                batch.ProductId,
                batch.WarehouseId,
                request.Status.ToString(),
                request.Reason,
                "Placed",
                currentUser.UserId,
                DateTime.UtcNow);
            var payload = JsonSerializer.Serialize(holdEvent);
            await _outboxRepository.AddAsync(
                OutboxMessage.Create("pharmacy.batch.held", payload),
                token);

            return Result.Ok();
        }, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "ok" });
    }

    [HttpPost("holds/release")]
    [RequireCompanyPermission(PermissionKeys.Inventory.AdjustmentsCreate)]
    public async Task<IActionResult> ReleaseHold(BatchReleaseRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var currentUser))
        {
            return Unauthorized();
        }

        if (request.CompanyId != currentUser.CompanyId)
        {
            return Forbid();
        }

        if (!CanManageHold(currentUser))
        {
            return Forbid();
        }

        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var batch = await _stockBatchRepository.GetByIdAsync(request.StockBatchId, token);
            if (batch is null || batch.CompanyId != currentUser.CompanyId)
            {
                return Result.Fail("Stock batch not found.");
            }

            var previousStatus = batch.HealthStatus;
            if (batch.HealthStatus == StockBatchHealthStatus.Blocked)
            {
                var activeBlock = await _blockHoldRepository.GetActiveByBatchAsync(currentUser.CompanyId, batch.Id, token);
                if (activeBlock is null)
                {
                    return Result.Fail("No active block found for this batch.");
                }

                activeBlock.Release(currentUser.UserId);
                await _blockHoldRepository.UpdateAsync(activeBlock, token);
            }
            else
            {
                var activeHold = await _quarantineHoldRepository.GetActiveByBatchAsync(currentUser.CompanyId, batch.Id, token);
                if (activeHold is null)
                {
                    return Result.Fail("No active quarantine/hold found for this batch.");
                }

                activeHold.Release(currentUser.UserId);
                await _quarantineHoldRepository.UpdateAsync(activeHold, token);
            }

            batch.ReleaseHold();
            await _stockBatchRepository.UpdateAsync(batch, token);
            await RecordCriticalAuditAsync(
                batch.CompanyId,
                nameof(StockBatch),
                batch.Id.ToString(),
                "BatchHoldReleased",
                new
                {
                    batch.Id,
                    batch.BatchNumber,
                    batch.ProductId,
                    batch.WarehouseId,
                    batch.HealthStatus
                },
                currentUser.Email,
                token);

            var holdEvent = new PharmaceuticalRegulatedInventoryBatchHeld(
                batch.CompanyId,
                batch.Id,
                batch.BatchNumber,
                batch.ProductId,
                batch.WarehouseId,
                previousStatus.ToString(),
                "Released",
                "Released",
                currentUser.UserId,
                DateTime.UtcNow);
            var payload = JsonSerializer.Serialize(holdEvent);
            await _outboxRepository.AddAsync(
                OutboxMessage.Create("pharmacy.batch.released", payload),
                token);

            return Result.Ok();
        }, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "ok" });
    }

    [HttpPost("quarantine/release")]
    [RequireCompanyPermission(PermissionKeys.Inventory.AdjustmentsCreate)]
    public async Task<IActionResult> ReleaseQuarantine(ReleaseQuarantineRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var currentUser))
        {
            return Unauthorized();
        }

        if (request.CompanyId != currentUser.CompanyId)
        {
            return Forbid();
        }

        if (!CanManageHold(currentUser))
        {
            return Forbid();
        }

        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var batch = await _stockBatchRepository.GetByIdAsync(request.StockBatchId, token);
            if (batch is null || batch.CompanyId != currentUser.CompanyId)
            {
                return Result.Fail("Stock batch not found.");
            }

            var activeHold = await _quarantineHoldRepository.GetActiveByBatchAsync(currentUser.CompanyId, batch.Id, token);
            if (activeHold is null)
            {
                return Result.Fail("No active quarantine/hold found for this batch.");
            }

            activeHold.Release(currentUser.UserId);
            batch.ReleaseHold();

            await _quarantineHoldRepository.UpdateAsync(activeHold, token);
            await _stockBatchRepository.UpdateAsync(batch, token);
            await RecordCriticalAuditAsync(
                batch.CompanyId,
                nameof(StockBatch),
                batch.Id.ToString(),
                "BatchHoldReleased",
                new
                {
                    batch.Id,
                    batch.BatchNumber,
                    batch.ProductId,
                    batch.WarehouseId,
                    activeHold.Status
                },
                currentUser.Email,
                token);
            return Result.Ok();
        }, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "ok" });
    }

    [HttpPost("receipts")]
    [RequireCompanyPermission(PermissionKeys.Inventory.AdjustmentsCreate)]
    public async Task<IActionResult> ReceiveInvoice(PurchaseInvoiceReceiptRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var currentUser))
        {
            return Unauthorized();
        }

        if (request.CompanyId != currentUser.CompanyId)
        {
            return Forbid();
        }

        if (request.Lines is null || request.Lines.Count == 0)
        {
            return BadRequest(new { error = "At least one invoice line is required." });
        }

        if (!string.IsNullOrWhiteSpace(request.QrPayload))
        {
            var parsed = _qrParser.TryParse(request.QrPayload);
            if (parsed is not null && !string.Equals(parsed.InvoiceNumber, request.InvoiceNumber, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { error = "QR payload invoice number does not match the submitted invoice." });
            }
        }

        foreach (var line in request.Lines)
        {
            var productBarcode = await _productBarcodeLookup.GetBarcodeByProductIdAsync(currentUser.CompanyId, line.ProductId, cancellationToken);
            if (string.IsNullOrWhiteSpace(productBarcode))
            {
                var productInfo = await _productPharmaInfoRepository.GetByProductAsync(currentUser.CompanyId, line.ProductId, cancellationToken);
                if (productInfo is null || string.IsNullOrWhiteSpace(productInfo.Barcode))
                {
                    return BadRequest(new { error = $"Product {line.ProductId} must have a barcode configured before receiving regulated inventory." });
                }
            }
        }

        var ingestion = PurchaseInvoiceIngestion.Create(
            currentUser.CompanyId,
            request.SupplierId,
            request.PurchaseOrderId,
            request.InvoiceNumber,
            request.IssuedAt,
            request.DueAt,
            new Money(request.NetAmount, request.Currency),
            new Money(request.TaxAmount, request.Currency),
            new Money(request.TotalAmount, request.Currency),
            request.QrPayload,
            PurchaseInvoiceIngestionStatus.Pending);

        foreach (var line in request.Lines)
        {
            ingestion.AddLine(
                line.ProductId,
                line.WarehouseId,
                line.Quantity,
                new Money(line.UnitPrice, request.Currency),
                line.BatchNumber,
                line.ExpiryDate,
                line.InboundPresentation);
        }

        await _ingestionRepository.AddAsync(ingestion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var receiptResult = await _purchasingService.ReceiveGoodsAsync(
            currentUser.CompanyId,
            request.SupplierId,
            request.PurchaseOrderId,
            request.Lines.Select(line => (line.ProductId, line.WarehouseId, line.Quantity, line.BatchNumber, line.ExpiryDate)),
            cancellationToken);

        if (!receiptResult.Success || receiptResult.Value is null)
        {
            ingestion.MarkFailed();
            await _ingestionRepository.UpdateAsync(ingestion, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return BadRequest(new { error = receiptResult.Error ?? "Failed to receive goods." });
        }

        ingestion.MarkReceived(receiptResult.Value.Id);
        await _ingestionRepository.UpdateAsync(ingestion, cancellationToken);

        if (request.PurchaseOrderId.HasValue)
        {
            await _payablesService.CreateFromGoodsReceiptAsync(
                currentUser.CompanyId,
                receiptResult.Value.Id,
                request.DueAt,
                Array.Empty<(DateTime dueDate, decimal amount)>(),
                cancellationToken);
        }

        var receiptEvent = new PharmaceuticalRegulatedInventoryReceiptPosted(
            currentUser.CompanyId,
            ingestion.Id,
            receiptResult.Value.Id,
            ingestion.SupplierId,
            ingestion.InvoiceNumber,
            ingestion.IssuedAt,
            ingestion.DueAt,
            ingestion.TotalAmount.Amount,
            ingestion.TotalAmount.Currency,
            DateTime.UtcNow);
        var payload = JsonSerializer.Serialize(receiptEvent);
        await _outboxRepository.AddAsync(
            OutboxMessage.Create("pharmacy.receipt.posted", payload),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(new { status = "ok", ingestionId = ingestion.Id, goodsReceiptId = receiptResult.Value.Id });
    }

    [HttpPost("recalls")]
    [RequireCompanyPermission(PermissionKeys.Inventory.AdjustmentsCreate)]
    public async Task<IActionResult> RegisterRecall(CreateRecallRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var currentUser))
        {
            return Unauthorized();
        }

        if (request.CompanyId != currentUser.CompanyId)
        {
            return Forbid();
        }

        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var batch = await _stockBatchRepository.GetByIdAsync(request.StockBatchId, token);
            if (batch is null || batch.CompanyId != currentUser.CompanyId)
            {
                return Result.Fail("Stock batch not found.");
            }

            if (request.Quantity is not null && request.Quantity > batch.QuantityOnHand)
            {
                return Result.Fail("Recall quantity exceeds available batch stock.");
            }

            var recall = Recall.Create(currentUser.CompanyId, batch.Id, request.Reason, request.Quantity, currentUser.UserId);
            batch.RecordRecall(request.Reason);

            await _recallRepository.AddAsync(recall, token);
            await _stockBatchRepository.UpdateAsync(batch, token);
            await RecordCriticalAuditAsync(
                batch.CompanyId,
                nameof(StockBatch),
                batch.Id.ToString(),
                "BatchRecalled",
                new
                {
                    batch.Id,
                    batch.BatchNumber,
                    batch.ProductId,
                    batch.WarehouseId,
                    request.Quantity,
                    request.Reason
                },
                currentUser.Email,
                token);
            await _traceEventRepository.AddAsync(PharmacyTraceEvent.CreateRecall(recall, batch), token);
            return Result.Ok();
        }, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "ok" });
    }

    [HttpPost("waste")]
    [RequireCompanyPermission(PermissionKeys.Inventory.AdjustmentsCreate)]
    public async Task<IActionResult> RegisterWaste(CreateWasteRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var currentUser))
        {
            return Unauthorized();
        }

        if (request.CompanyId != currentUser.CompanyId)
        {
            return Forbid();
        }

        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var batch = await _stockBatchRepository.GetByIdAsync(request.StockBatchId, token);
            if (batch is null || batch.CompanyId != currentUser.CompanyId)
            {
                return Result.Fail("Stock batch not found.");
            }

            if (request.Quantity > batch.QuantityOnHand)
            {
                return Result.Fail("Waste quantity exceeds available batch stock.");
            }

            var waste = Waste.Create(currentUser.CompanyId, batch.Id, request.Quantity, request.Reason, currentUser.UserId);
            batch.Decrease(request.Quantity);
            batch.RecordWaste(request.Reason);

            await _wasteRepository.AddAsync(waste, token);
            await _stockBatchRepository.UpdateAsync(batch, token);
            await RecordCriticalAuditAsync(
                batch.CompanyId,
                nameof(StockBatch),
                batch.Id.ToString(),
                "BatchWasteReported",
                new
                {
                    batch.Id,
                    batch.BatchNumber,
                    batch.ProductId,
                    batch.WarehouseId,
                    request.Quantity,
                    request.Reason
                },
                currentUser.Email,
                token);
            return Result.Ok();
        }, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "ok" });
    }

    [HttpPost("returns")]
    [RequireCompanyPermission(PermissionKeys.Inventory.AdjustmentsCreate)]
    public async Task<IActionResult> RegisterReturn(RegisterReturnRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var currentUser))
        {
            return Unauthorized();
        }

        if (request.CompanyId != currentUser.CompanyId)
        {
            return Forbid();
        }

        var referenceId = string.IsNullOrWhiteSpace(request.ReferenceId)
            ? Guid.NewGuid().ToString("N")
            : request.ReferenceId.Trim();

        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var movementResult = await _inventoryFlowService.RecordStockInAsync(
                currentUser.CompanyId,
                request.ProductId,
                request.WarehouseId,
                request.Quantity,
                "Return",
                referenceId,
                request.Reason,
                new[] { new StockMovementBatchInput(request.BatchNumber, request.ExpiryDate, request.Quantity) },
                new StockMovementReference(null, request.CustomerId, request.PatientReference),
                token);

            if (!movementResult.Success)
            {
                return movementResult;
            }

            return Result.Ok();
        }, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "ok", referenceId });
    }

    [HttpGet("trace/batch/{batchNumber}")]
    [RequireCompanyPermission(PermissionKeys.Inventory.StockRead)]
    public async Task<IActionResult> GetTraceByBatch(string batchNumber, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var currentUser))
        {
            return Unauthorized();
        }

        var traces = await _traceEventRepository.ListByBatchAsync(currentUser.CompanyId, batchNumber, cancellationToken);
        return Ok(traces.Select(MapTrace));
    }

    [HttpGet("trace/patient/{patientReference}")]
    [RequireCompanyPermission(PermissionKeys.Inventory.StockRead)]
    public async Task<IActionResult> GetTraceByPatient(string patientReference, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var currentUser))
        {
            return Unauthorized();
        }

        var traces = await _traceEventRepository.ListByPatientAsync(currentUser.CompanyId, patientReference, cancellationToken);
        return Ok(traces.Select(MapTrace));
    }

    [HttpGet("trace/supplier/{supplierId:long}")]
    [RequireCompanyPermission(PermissionKeys.Inventory.StockRead)]
    public async Task<IActionResult> GetTraceBySupplier(long supplierId, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var currentUser))
        {
            return Unauthorized();
        }

        var traces = await _traceEventRepository.ListBySupplierAsync(currentUser.CompanyId, supplierId, cancellationToken);
        return Ok(traces.Select(MapTrace));
    }

    private bool TryGetCurrentUser(out CurrentUser currentUser)
    {
        var resolved = _currentUserProvider.GetCurrentUser();
        if (resolved is null)
        {
            currentUser = null!;
            return false;
        }

        currentUser = resolved;
        return true;
    }

    private Task RecordCriticalAuditAsync(
        long companyId,
        string entityName,
        string entityId,
        string action,
        object payload,
        string recordedBy,
        CancellationToken cancellationToken)
    {
        var payloadJson = JsonSerializer.Serialize(payload);
        var audit = PharmacyCriticalAudit.Create(
            companyId,
            entityName,
            entityId,
            action,
            payloadJson,
            recordedBy,
            DateTime.UtcNow);
        return _criticalAuditRepository.AddAsync(audit, cancellationToken);
    }

    private static bool CanManageHold(CurrentUser currentUser)
    {
        return currentUser.Roles.Contains(RoleNames.OperationsManager) ||
            currentUser.Roles.Contains(RoleNames.PharmaceuticalChemist) ||
            currentUser.Roles.Contains(RoleNames.Pharmacist);
    }

    private static PharmaceuticalRegulatedInventoryTraceEventResponse MapTrace(PharmacyTraceEvent trace)
    {
        return new PharmaceuticalRegulatedInventoryTraceEventResponse(
            trace.Id,
            trace.StockBatchId,
            trace.ProductId,
            trace.WarehouseId,
            trace.BatchNumber,
            trace.ExpiryDate,
            trace.Quantity,
            trace.MovementType?.ToString(),
            trace.ReferenceType,
            trace.ReferenceId,
            trace.SupplierId,
            trace.CustomerId,
            trace.PatientReference,
            trace.Reason,
            trace.CreatedAt);
    }
}
