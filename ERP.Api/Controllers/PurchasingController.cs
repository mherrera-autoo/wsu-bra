using ERP.Api.Authorization;
using ERP.Api.Contracts.Purchasing;
using ERP.Modules.Purchasing.Application.Services;
using ERP.Modules.Purchasing.Domain;
using ERP.Shared.Application;
using ERP.Shared.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/purchasing")]
public sealed class PurchasingController : ControllerBase
{
    private readonly PurchasingService _purchasingService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public PurchasingController(PurchasingService purchasingService, ICurrentUserProvider currentUserProvider)
    {
        _purchasingService = purchasingService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpPost("purchase-orders")]
    public async Task<IActionResult> CreatePurchaseOrder(CreatePurchaseOrderRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var lines = request.Lines.Select(line =>
            (line.ProductId,
                line.Qty,
                new Money(line.UnitPriceAmount, line.UnitPriceCurrency),
                line.TaxGroupId));

        var result = await _purchasingService.CreatePurchaseOrderAsync(
            companyId,
            request.SupplierId,
            request.Currency,
            lines,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpGet("purchase-orders")]
    public async Task<ActionResult<IReadOnlyList<PurchaseOrderSummary>>> ListPurchaseOrders(CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var orders = await _purchasingService.ListPurchaseOrdersAsync(companyId, cancellationToken);
        var response = orders.Select(order => new PurchaseOrderSummary(
            order.Id,
            order.SupplierId,
            order.Currency,
            order.Status.ToString(),
            order.CreatedAt));
        return Ok(response);
    }

    [HttpGet("purchase-orders/{id:long}")]
    public async Task<ActionResult<PurchaseOrderDetail>> GetPurchaseOrder(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var order = await _purchasingService.GetPurchaseOrderAsync(companyId, id, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        var lines = order.Lines.Select(line => new PurchaseOrderLineDetail(
            line.ProductId,
            line.OrderedQty,
            line.UnitPriceRef.Amount,
            line.UnitPriceRef.Currency));

        return Ok(new PurchaseOrderDetail(
            order.Id,
            order.SupplierId,
            order.Currency,
            order.Status.ToString(),
            order.CreatedAt,
            lines.ToList()));
    }

    [HttpPost("goods-receipts")]
    public async Task<IActionResult> ReceiveGoods(CreateGoodsReceiptRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var lines = request.Lines.Select(line => (line.ProductId, line.WarehouseId, line.ReceivedQty, line.BatchNumber, line.ExpiryDate));

        var result = await _purchasingService.ReceiveGoodsAsync(
            companyId,
            request.SupplierId,
            request.PurchaseOrderId,
            lines,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpGet("goods-receipts")]
    public async Task<ActionResult<IReadOnlyList<GoodsReceiptSummary>>> ListGoodsReceipts(CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var receipts = await _purchasingService.ListGoodsReceiptsAsync(companyId, cancellationToken);
        var response = receipts.Select(receipt => new GoodsReceiptSummary(
            receipt.Id,
            receipt.SupplierId,
            receipt.PurchaseOrderId,
            receipt.ReceivedAt));
        return Ok(response);
    }

    [HttpGet("goods-receipts/{id:long}")]
    public async Task<ActionResult<GoodsReceiptDetail>> GetGoodsReceipt(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var receipt = await _purchasingService.GetGoodsReceiptAsync(companyId, id, cancellationToken);
        if (receipt is null)
        {
            return NotFound();
        }

        var lines = receipt.Lines.Select(line => new GoodsReceiptLineDetail(
            line.ProductId,
            line.WarehouseId,
            line.ReceivedQty));

        return Ok(new GoodsReceiptDetail(
            receipt.Id,
            receipt.SupplierId,
            receipt.PurchaseOrderId,
            receipt.ReceivedAt,
            lines.ToList()));
    }

    [HttpGet("suggestions")]
    public async Task<ActionResult<IReadOnlyList<PurchaseSuggestionResponse>>> ListPurchaseSuggestions(
        [FromQuery] long? warehouseId,
        [FromQuery] long? supplierId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        PurchaseSuggestionStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse(status, true, out PurchaseSuggestionStatus parsed))
            {
                return BadRequest(new { error = "Invalid status." });
            }

            parsedStatus = parsed;
        }

        var suggestions = await _purchasingService.ListPurchaseSuggestionsAsync(
            companyId,
            warehouseId,
            supplierId,
            parsedStatus,
            cancellationToken);

        var response = suggestions.Select(MapSuggestion).ToList();
        return Ok(response);
    }

    [HttpPost("suggestions")]
    public async Task<IActionResult> CreatePurchaseSuggestion(CreatePurchaseSuggestionRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var lines = new List<PurchaseSuggestionLineInput>();
        foreach (var line in request.Lines)
        {
            if (!TryParseReason(line.Reason, out var reason))
            {
                return BadRequest(new { error = $"Invalid reason '{line.Reason}'." });
            }

            lines.Add(new PurchaseSuggestionLineInput(
                line.ProductId,
                line.QtySuggested,
                line.SupplierSuggestedId,
                reason,
                line.WarehouseId));
        }

        var result = await _purchasingService.CreatePurchaseSuggestionAsync(
            companyId,
            new PurchaseSuggestionCreateInput(request.SupplierSuggestedId, lines),
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpPut("suggestions/{id:long}")]
    public async Task<IActionResult> UpdatePurchaseSuggestion(long id, UpdatePurchaseSuggestionRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var lines = new List<PurchaseSuggestionLineInput>();
        foreach (var line in request.Lines)
        {
            if (!TryParseReason(line.Reason, out var reason))
            {
                return BadRequest(new { error = $"Invalid reason '{line.Reason}'." });
            }

            lines.Add(new PurchaseSuggestionLineInput(
                line.ProductId,
                line.QtySuggested,
                line.SupplierSuggestedId,
                reason,
                line.WarehouseId));
        }

        var result = await _purchasingService.UpdatePurchaseSuggestionAsync(
            companyId,
            id,
            new PurchaseSuggestionUpdateInput(lines),
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpPost("suggestions/{id:long}/approve")]
    public async Task<IActionResult> ApprovePurchaseSuggestion(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _purchasingService.ApprovePurchaseSuggestionAsync(companyId, id, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok();
    }

    [HttpPost("suggestions/{id:long}/reject")]
    public async Task<IActionResult> RejectPurchaseSuggestion(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _purchasingService.RejectPurchaseSuggestionAsync(companyId, id, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok();
    }

    [HttpPost("suggestions/{id:long}/convert-to-po")]
    public async Task<IActionResult> ConvertPurchaseSuggestion(long id, ConvertPurchaseSuggestionRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _purchasingService.ConvertPurchaseSuggestionToPurchaseOrderAsync(
            companyId,
            id,
            new PurchaseSuggestionConvertInput(request.SupplierId, request.Currency),
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpPost("suggestions/generate")]
    public async Task<IActionResult> GeneratePurchaseSuggestions(GeneratePurchaseSuggestionsRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var minimums = request.Minimums?.Select(min => new PurchaseSuggestionMinimumInput(
                min.ProductId,
                min.MinimumQty,
                min.WarehouseId))
            .ToList() ?? new List<PurchaseSuggestionMinimumInput>();

        var result = await _purchasingService.GeneratePurchaseSuggestionsAsync(
            companyId,
            new PurchaseSuggestionGenerateInput(request.WarehouseId, request.SupplierId, minimums),
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        if (result.Value is null)
        {
            return Ok(new { message = "No suggestions generated." });
        }

        return Ok(new { result.Value.Id });
    }

    private bool TryGetCompanyId(out long companyId)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null)
        {
            companyId = default;
            return false;
        }

        companyId = currentUser.CompanyId;
        return true;
    }

    private static bool TryParseReason(string reason, out PurchaseSuggestionReason parsed)
        => Enum.TryParse(reason, true, out parsed);

    private static PurchaseSuggestionResponse MapSuggestion(PurchaseSuggestion suggestion)
    {
        var lines = suggestion.Lines.Select(line => new PurchaseSuggestionLineResponse(
            line.ProductId,
            line.QtySuggested,
            line.SupplierSuggestedId,
            line.Reason.ToString(),
            line.WarehouseId)).ToList();

        return new PurchaseSuggestionResponse(
            suggestion.Id,
            suggestion.Status.ToString(),
            suggestion.SupplierSuggestedId,
            suggestion.ConvertedPurchaseOrderId,
            suggestion.CreatedAt,
            lines);
    }
}
