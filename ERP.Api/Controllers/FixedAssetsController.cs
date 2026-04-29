using ERP.Api.Authorization;
using ERP.Modules.FixedAssets.Application.Services;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/fixed-assets")]
public sealed class FixedAssetsController : ControllerBase
{
    private readonly FixedAssetService _fixedAssetService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public FixedAssetsController(FixedAssetService fixedAssetService, ICurrentUserProvider currentUserProvider)
    {
        _fixedAssetService = fixedAssetService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var category = await _fixedAssetService.CreateCategoryAsync(
            companyId,
            request.Name,
            request.UsefulLifeMonths,
            request.AssetAccountId,
            request.AccumulatedDepreciationAccountId,
            request.DepreciationExpenseAccountId,
            cancellationToken);

        return Ok(new
        {
            category.Id,
            companyId,
            category.Name,
            category.UsefulLifeMonths,
            category.AssetAccountId,
            category.AccumulatedDepreciationAccountId,
            category.DepreciationExpenseAccountId
        });
    }

    [HttpPost]
    public async Task<IActionResult> RegisterAsset([FromBody] CreateFixedAssetRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var asset = await _fixedAssetService.RegisterAssetAsync(
            companyId,
            request.Name,
            request.CategoryId,
            request.AcquisitionDate,
            request.Cost,
            request.SalvageValue,
            cancellationToken);

        return Ok(new
        {
            asset.Id,
            companyId,
            asset.Name,
            asset.CategoryId,
            asset.AcquisitionDate,
            asset.Cost,
            asset.SalvageValue,
            asset.UsefulLifeMonths,
            asset.Status
        });
    }

    [HttpPost("{assetId:long}/dispose")]
    public async Task<IActionResult> DisposeAsset(long assetId, [FromBody] DisposeFixedAssetRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        await _fixedAssetService.DisposeAssetAsync(companyId, assetId, request.DisposalDate, cancellationToken);
        return Ok(new { assetId, companyId, request.DisposalDate });
    }

    [HttpPost("depreciation/calculate")]
    public async Task<IActionResult> CalculateDepreciation([FromBody] CalculateDepreciationRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var correlationIds = await _fixedAssetService.CalculateDepreciationAsync(companyId, request.AsOfDate, cancellationToken);
        return Ok(new
        {
            companyId,
            request.AsOfDate,
            correlationIds
        });
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

    public sealed record CreateCategoryRequest(
        long CompanyId,
        string Name,
        int UsefulLifeMonths,
        long AssetAccountId,
        long AccumulatedDepreciationAccountId,
        long DepreciationExpenseAccountId);

    public sealed record CreateFixedAssetRequest(
        long CompanyId,
        string Name,
        long CategoryId,
        DateTime AcquisitionDate,
        decimal Cost,
        decimal SalvageValue);

    public sealed record DisposeFixedAssetRequest(long CompanyId, DateTime DisposalDate);

    public sealed record CalculateDepreciationRequest(long CompanyId, DateTime AsOfDate);
}
