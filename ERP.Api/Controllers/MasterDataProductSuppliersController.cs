using ERP.Api.Authorization;
using ERP.Api.Contracts.MasterData;
using ERP.Modules.MasterData.Application.Commands;
using ERP.Modules.MasterData.Application.Handlers;
using ERP.Modules.MasterData.Application.Queries;
using ERP.Modules.MasterData.Domain;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/masterdata/product-suppliers")]
public sealed class MasterDataProductSuppliersController : ControllerBase
{
    private readonly ProductSupplierCommandHandler _commandHandler;
    private readonly ProductSupplierQueryHandler _queryHandler;
    private readonly ICurrentUserProvider _currentUserProvider;

    public MasterDataProductSuppliersController(
        ProductSupplierCommandHandler commandHandler,
        ProductSupplierQueryHandler queryHandler,
        ICurrentUserProvider currentUserProvider)
    {
        _commandHandler = commandHandler;
        _queryHandler = queryHandler;
        _currentUserProvider = currentUserProvider;
    }

    [HttpPut("products/{productId:long}/suppliers/{supplierId:long}")]
    public async Task<IActionResult> Upsert(
        long productId,
        long supplierId,
        UpsertProductSupplierRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _commandHandler.HandleAsync(
            new UpsertProductSupplierCommand(
                companyId,
                productId,
                supplierId,
                request.SupplierSku,
                request.UnitPurchasePrice,
                request.MinimumPurchaseLot),
            cancellationToken);

        if (!result.Success)
        {
            if (string.Equals(result.Error, "Product not found.", StringComparison.OrdinalIgnoreCase)
                || string.Equals(result.Error, "Supplier not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(ToSummary(result.Value!));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ProductSupplierSummary>> GetById(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var productSupplier = await _queryHandler.HandleAsync(new GetProductSupplierByIdQuery(companyId, id), cancellationToken);
        if (productSupplier is null)
        {
            return NotFound();
        }

        return Ok(ToSummary(productSupplier));
    }

    [HttpGet("products/{productId:long}/suppliers/{supplierId:long}")]
    public async Task<ActionResult<ProductSupplierSummary>> GetByPair(long productId, long supplierId, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var productSupplier = await _queryHandler.HandleAsync(new GetProductSupplierByPairQuery(companyId, productId, supplierId), cancellationToken);
        if (productSupplier is null)
        {
            return NotFound();
        }

        return Ok(ToSummary(productSupplier));
    }

    [HttpGet("products/{productId:long}")]
    public async Task<ActionResult<IReadOnlyList<ProductSupplierSummary>>> ListByProduct(long productId, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var productSuppliers = await _queryHandler.HandleAsync(new ListProductSuppliersByProductQuery(companyId, productId), cancellationToken);
        return Ok(productSuppliers.Select(ToSummary));
    }

    [HttpGet("suppliers/{supplierId:long}")]
    public async Task<ActionResult<IReadOnlyList<ProductSupplierSummary>>> ListBySupplier(long supplierId, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var productSuppliers = await _queryHandler.HandleAsync(new ListProductSuppliersBySupplierQuery(companyId, supplierId), cancellationToken);
        return Ok(productSuppliers.Select(ToSummary));
    }

    [HttpDelete("products/{productId:long}/suppliers/{supplierId:long}")]
    public async Task<IActionResult> Delete(long productId, long supplierId, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _commandHandler.HandleAsync(
            new DeleteProductSupplierCommand(companyId, productId, supplierId),
            cancellationToken);

        if (!result.Success)
        {
            if (string.Equals(result.Error, "ProductSupplier not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "deleted" });
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

    private static ProductSupplierSummary ToSummary(ProductSupplier productSupplier)
        => new(
            productSupplier.Id,
            productSupplier.ProductId,
            productSupplier.Product?.Sku,
            productSupplier.Product?.Name,
            productSupplier.SupplierId,
            productSupplier.Supplier?.Name,
            productSupplier.SupplierSku,
            productSupplier.UnitPurchasePrice,
            productSupplier.MinimumPurchaseLot,
            productSupplier.PublicId,
            productSupplier.CreatedAt,
            productSupplier.UpdatedAt);
}
