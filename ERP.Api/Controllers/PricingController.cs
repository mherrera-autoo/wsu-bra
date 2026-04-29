using ERP.Api.Authorization;
using ERP.Api.Contracts.Pricing;
using ERP.Modules.Pricing.Application.Services;
using ERP.Shared.Application;
using ERP.Shared.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/pricing")]
public sealed class PricingController : ControllerBase
{
    private readonly PricingService _pricingService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public PricingController(PricingService pricingService, ICurrentUserProvider currentUserProvider)
    {
        _pricingService = pricingService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet("price-lists")]
    public async Task<ActionResult<IReadOnlyList<PriceListSummary>>> ListPriceLists(CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var priceLists = await _pricingService.ListPriceListsAsync(companyId, cancellationToken);
        var response = priceLists.Select(list => new PriceListSummary(
            list.Id,
            list.Name,
            list.IsDefault));
        return Ok(response);
    }

    [HttpGet("price-lists/{id:long}")]
    public async Task<ActionResult<PriceListDetail>> GetPriceList(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var priceList = await _pricingService.GetPriceListAsync(companyId, id, cancellationToken);
        if (priceList is null)
        {
            return NotFound();
        }

        var items = await _pricingService.ListPriceListItemsAsync(companyId, id, cancellationToken);
        var responseItems = items.Select(item => new PriceListItemSummary(
            item.Id,
            item.ProductId,
            item.UnitPrice.Amount,
            item.UnitPrice.Currency))
            .ToList();

        return Ok(new PriceListDetail(
            priceList.Id,
            priceList.Name,
            priceList.IsDefault,
            responseItems));
    }

    [HttpPost("price-lists")]
    public async Task<IActionResult> CreatePriceList(CreatePriceListRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var result = await _pricingService.CreatePriceListAsync(
            companyId,
            request.Name,
            request.IsDefault,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpDelete("price-lists/{id:long}")]
    public async Task<IActionResult> DeletePriceList(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _pricingService.DeletePriceListAsync(companyId, id, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "deleted" });
    }

    [HttpGet("price-lists/{id:long}/items")]
    public async Task<ActionResult<IReadOnlyList<PriceListItemSummary>>> ListPriceListItems(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var items = await _pricingService.ListPriceListItemsAsync(companyId, id, cancellationToken);
        var response = items.Select(item => new PriceListItemSummary(
            item.Id,
            item.ProductId,
            item.UnitPrice.Amount,
            item.UnitPrice.Currency));
        return Ok(response);
    }

    [HttpPost("price-lists/{id:long}/items")]
    public async Task<IActionResult> AddPriceListItem(long id, CreatePriceListItemRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        if (request.PriceListId != id)
        {
            return BadRequest(new { error = "Price list mismatch." });
        }

        var result = await _pricingService.AddPriceAsync(
            companyId,
            id,
            request.ProductId,
            new Money(request.UnitPriceAmount, request.UnitPriceCurrency),
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpDelete("price-lists/{id:long}/items/{productId:long}")]
    public async Task<IActionResult> RemovePriceListItem(long id, long productId, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _pricingService.RemovePriceAsync(companyId, id, productId, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "deleted" });
    }

    [HttpPut("price-lists/{id:long}/items/{productId:long}")]
    public async Task<IActionResult> UpdatePriceListItem(
        long id,
        long productId,
        UpdatePriceListItemRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        if (request.PriceListId != id || request.ProductId != productId)
        {
            return BadRequest(new { error = "Price list item mismatch." });
        }

        var result = await _pricingService.UpdatePriceAsync(
            companyId,
            id,
            productId,
            new Money(request.UnitPriceAmount, request.UnitPriceCurrency),
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "updated" });
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
}
