using ERP.Api.Authorization;
using ERP.Api.Contracts.Companies;
using ERP.Modules.Identity.Contracts;
using ERP.Modules.MasterData.Application.Services;
using ERP.Modules.MasterData.Domain;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/companies/features")]
public sealed class CompanyFeaturesController : ControllerBase
{
    private readonly ICompanyFeatureService _companyFeatureService;
    private readonly IFeatureCatalogService _featureCatalogService;
    private readonly ICompanyContext _companyContext;
    private readonly ICurrentUserProvider _currentUserProvider;

    public CompanyFeaturesController(
        ICompanyFeatureService companyFeatureService,
        IFeatureCatalogService featureCatalogService,
        ICompanyContext companyContext,
        ICurrentUserProvider currentUserProvider)
    {
        _companyFeatureService = companyFeatureService;
        _featureCatalogService = featureCatalogService;
        _companyContext = companyContext;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet]
    [ProducesResponseType(typeof(CompanyFeaturesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        if (_companyContext.CompanyId <= 0)
        {
            return BadRequest(new { error = "Company context is required." });
        }

        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser?.CompanyPublicId is null)
        {
            return BadRequest(new { error = "Company public context is required." });
        }

        var enabledFeaturePublicIds = await _companyFeatureService.ListEnabledFeaturePublicIds(cancellationToken);
        var enabledFeatures = enabledFeaturePublicIds
            .Select(featurePublicId => FeatureCode.TryFromPublicId(featurePublicId, out var code) ? code.Code : null)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code!)
            .ToArray();

        var response = new CompanyFeaturesResponse(currentUser.CompanyPublicId.Value, enabledFeatures);

        return Ok(response);
    }

    [HttpPost("{featurePublicId:guid}:enable")]
    [RequireCompanyPermission(PermissionKeys.Platform.FeaturesManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Enable(
        Guid featurePublicId,
        [FromBody] CompanyFeatureToggleRequest request,
        CancellationToken cancellationToken)
    {
        if (featurePublicId == Guid.Empty)
        {
            return BadRequest(new { error = "FeaturePublicId is required." });
        }

        if (_companyContext.CompanyId <= 0 || _companyContext.UserId <= 0)
        {
            return BadRequest(new { error = "Company context is required." });
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var catalogFeature = await _featureCatalogService.GetByPublicId(featurePublicId, cancellationToken);
        if (catalogFeature is null || !catalogFeature.IsActive)
        {
            return NotFound(new { error = "Feature not found in active catalog." });
        }

        await _companyFeatureService.EnableFeature(
            featurePublicId,
            _companyContext.UserId,
            request.Reason,
            request.CorrelationId,
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{featurePublicId:guid}:disable")]
    [RequireCompanyPermission(PermissionKeys.Platform.FeaturesManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Disable(
        Guid featurePublicId,
        [FromBody] CompanyFeatureToggleRequest request,
        CancellationToken cancellationToken)
    {
        if (featurePublicId == Guid.Empty)
        {
            return BadRequest(new { error = "FeaturePublicId is required." });
        }

        if (_companyContext.CompanyId <= 0 || _companyContext.UserId <= 0)
        {
            return BadRequest(new { error = "Company context is required." });
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var catalogFeature = await _featureCatalogService.GetByPublicId(featurePublicId, cancellationToken);
        if (catalogFeature is null || !catalogFeature.IsActive)
        {
            return NotFound(new { error = "Feature not found in active catalog." });
        }

        await _companyFeatureService.DisableFeature(
            featurePublicId,
            _companyContext.UserId,
            request.Reason,
            request.CorrelationId,
            cancellationToken);

        return NoContent();
    }
}
