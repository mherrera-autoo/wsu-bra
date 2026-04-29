using ERP.Api.Authorization;
using ERP.Api.Filters;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[RequireFeature(PharmacyFeatureKeys.Base)]
[Route("api/pharmaceutical-regulated-inventory/products")]
public sealed class PharmaceuticalRegulatedInventoryProductsController : ControllerBase
{
    private readonly IProductPharmaInfoRepository _infoRepository;
    private readonly IProductBarcodeLookup _productBarcodeLookup;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserProvider _currentUserProvider;

    public PharmaceuticalRegulatedInventoryProductsController(
        IProductPharmaInfoRepository infoRepository,
        IProductBarcodeLookup productBarcodeLookup,
        IUnitOfWork unitOfWork,
        ICurrentUserProvider currentUserProvider)
    {
        _infoRepository = infoRepository;
        _productBarcodeLookup = productBarcodeLookup;
        _unitOfWork = unitOfWork;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet("{productId:long}/pharma-info")]
    [RequireCompanyPermission(PermissionKeys.Inventory.StockRead)]
    public async Task<IActionResult> GetPharmaInfo(long productId, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var info = await _infoRepository.GetByProductAsync(companyId, productId, cancellationToken);
        if (info is null)
        {
            return Ok(null);
        }

        var productBarcode = await _productBarcodeLookup.GetBarcodeByProductIdAsync(companyId, productId, cancellationToken);

        return Ok(new
        {
            info.Id,
            info.ProductId,
            info.Laboratory,
            info.Presentation,
            info.ActiveIngredient,
            info.IsBioequivalent,
            info.CenabastCode,
            Barcode = productBarcode ?? info.Barcode
        });
    }

    [HttpGet("barcode/{barcode}")]
    [RequireCompanyPermission(PermissionKeys.Inventory.StockRead)]
    public async Task<IActionResult> GetByBarcode(string barcode, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var productMatch = await _productBarcodeLookup.GetMatchByBarcodeAsync(companyId, barcode, cancellationToken);
        if (productMatch is not null)
        {
            var infoFromProduct = await _infoRepository.GetByProductAsync(companyId, productMatch.ProductId, cancellationToken);
            return Ok(new
            {
                ProductId = productMatch.ProductId,
                Barcode = productMatch.Barcode,
                Laboratory = infoFromProduct?.Laboratory,
                Presentation = infoFromProduct?.Presentation
            });
        }

        var info = await _infoRepository.GetByBarcodeAsync(companyId, barcode, cancellationToken);
        if (info is null)
        {
            return NotFound(new { error = "Barcode not found." });
        }

        var productBarcode = await _productBarcodeLookup.GetBarcodeByProductIdAsync(companyId, info.ProductId, cancellationToken);

        return Ok(new
        {
            ProductId = info.ProductId,
            Barcode = productBarcode ?? info.Barcode,
            Laboratory = info?.Laboratory,
            Presentation = info?.Presentation
        });
    }

    [HttpPost("{productId:long}/pharma-info")]
    [RequireCompanyPermission(PermissionKeys.Inventory.AdjustmentsCreate)]
    public async Task<IActionResult> UpsertPharmaInfo(long productId, [FromBody] ProductPharmaInfoRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(request.Barcode))
        {
            return BadRequest(new { error = "Barcode is required for regulated pharmaceutical products." });
        }

        var existing = await _infoRepository.GetByProductAsync(companyId, productId, cancellationToken);
        if (existing is null)
        {
            var info = ProductPharmaInfo.Create(
                companyId,
                productId,
                request.Laboratory,
                request.Presentation,
                request.ActiveIngredient,
                request.IsBioequivalent,
                request.CenabastCode,
                request.Barcode);
            await _infoRepository.AddAsync(info, cancellationToken);
        }
        else
        {
            existing.Update(
                request.Laboratory,
                request.Presentation,
                request.ActiveIngredient,
                request.IsBioequivalent,
                request.CenabastCode,
                request.Barcode);
            await _infoRepository.UpdateAsync(existing, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(new { status = "ok" });
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

public sealed record ProductPharmaInfoRequest(
    long CompanyId,
    string Laboratory,
    string Presentation,
    string? ActiveIngredient,
    bool IsBioequivalent,
    string? CenabastCode,
    string? Barcode);
