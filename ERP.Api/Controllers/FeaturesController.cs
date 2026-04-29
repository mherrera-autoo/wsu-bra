using ERP.Api.Authorization;
using ERP.Api.Contracts.Features;
using ERP.Modules.Identity.Contracts;
using ERP.Modules.MasterData.Application.Services;
using ERP.Shared.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/features")]
public sealed class FeaturesController : ControllerBase
{
    private readonly IFeatureCatalogService _featureCatalogService;
    private readonly ICompanyFeatureService _companyFeatureService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public FeaturesController(
        IFeatureCatalogService featureCatalogService,
        ICompanyFeatureService companyFeatureService,
        ICurrentUserProvider currentUserProvider)
    {
        _featureCatalogService = featureCatalogService;
        _companyFeatureService = companyFeatureService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet("{featurePublicId:guid}")]
    [ProducesResponseType(typeof(FeatureCatalogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCatalogFeature(Guid featurePublicId, CancellationToken cancellationToken)
    {
        if (featurePublicId == Guid.Empty)
        {
            return BadRequest(new { error = "FeaturePublicId is required." });
        }

        var catalogFeature = await _featureCatalogService.GetByPublicId(featurePublicId, cancellationToken);
        if (catalogFeature is null || !catalogFeature.IsActive)
        {
            return NotFound(new { error = "Feature not found in catalog." });
        }

        var assignedFeaturePublicIds = await GetAssignedFeaturePublicIds(cancellationToken);

        return Ok(ToCatalogResponse(catalogFeature, assignedFeaturePublicIds.Contains(catalogFeature.PublicId)));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FeatureCatalogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListFeatures(CancellationToken cancellationToken)
    {
        var features = await _featureCatalogService.ListAll(cancellationToken);
        var assignedFeaturePublicIds = await GetAssignedFeaturePublicIds(cancellationToken);

        var response = features
            .Select(feature => ToCatalogResponse(feature, assignedFeaturePublicIds.Contains(feature.PublicId)))
            .ToArray();

        return Ok(response);
    }

    [HttpPost("{featurePublicId:guid}:enable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnableFeature(
        Guid featurePublicId,
        [FromBody] FeatureChangeRequest request,
        CancellationToken cancellationToken)
    {
        if (featurePublicId == Guid.Empty)
        {
            return BadRequest(new { error = "FeaturePublicId is required." });
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var updated = await _featureCatalogService.SetActive(featurePublicId, isActive: true, cancellationToken);
        if (!updated)
        {
            return NotFound(new { error = "Feature not found in catalog." });
        }

        return NoContent();
    }

    [HttpPost("{featurePublicId:guid}:disable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DisableFeature(
        Guid featurePublicId,
        [FromBody] FeatureChangeRequest request,
        CancellationToken cancellationToken)
    {
        if (featurePublicId == Guid.Empty)
        {
            return BadRequest(new { error = "FeaturePublicId is required." });
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var updated = await _featureCatalogService.SetActive(featurePublicId, isActive: false, cancellationToken);
        if (!updated)
        {
            return NotFound(new { error = "Feature not found in catalog." });
        }

        return NoContent();
    }

    [HttpPost]
    [RequirePlatformPermission(PermissionKeys.Platform.FeaturesManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpsertCompanyFeature(
        [FromBody] PlatformCompanyFeatureUpsertRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (request.FeaturePublicId == Guid.Empty)
        {
            return BadRequest(new { error = "FeaturePublicId is required." });
        }

        if (request.CompanyPublicId == Guid.Empty)
        {
            return BadRequest(new { error = "CompanyPublicId is required." });
        }

        var catalogFeature = await _featureCatalogService.GetByPublicId(request.FeaturePublicId, cancellationToken);
        if (catalogFeature is null || !catalogFeature.IsActive)
        {
            return NotFound(new { error = "Feature not found in active catalog." });
        }

        var userId = _currentUserProvider.GetCurrentUser()?.UserId ?? 0;
        if (userId <= 0)
        {
            return BadRequest(new { error = "User context is required." });
        }

        try
        {
            await _companyFeatureService.UpsertCompanyFeature(
                request.CompanyPublicId,
                request.FeaturePublicId,
                request.IsActive,
                userId,
                request.Reason,
                request.CorrelationId,
                cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { error = exception.Message });
        }
        catch (DbUpdateException exception)
        {
            var detail = exception.InnerException?.Message ?? exception.Message;
            return Conflict(new
            {
                error = "Could not persist company feature state.",
                detail
            });
        }

        return NoContent();
    }

    private async Task<HashSet<Guid>> GetAssignedFeaturePublicIds(CancellationToken cancellationToken)
    {
        var companyId = _currentUserProvider.GetCurrentUser()?.CompanyId ?? 0;
        if (companyId <= 0)
        {
            return new HashSet<Guid>();
        }

        var assignedFeaturePublicIds = await _companyFeatureService.ListEnabledFeaturePublicIds(cancellationToken);
        return assignedFeaturePublicIds.ToHashSet();
    }

    private static FeatureCatalogResponse ToCatalogResponse(
        ERP.Modules.MasterData.Domain.FeatureCatalog feature,
        bool isAssignedToCompany)
    {
        return new FeatureCatalogResponse(
            feature.PublicId,
            feature.Code,
            feature.Name,
            feature.Description,
            feature.IsActive,
            isAssignedToCompany);
    }
}
