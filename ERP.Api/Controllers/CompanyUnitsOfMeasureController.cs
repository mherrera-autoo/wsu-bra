using System.Globalization;
using ERP.Api.Authorization;
using ERP.Api.Contracts.Companies;
using ERP.Modules.MasterData.Application.Services;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/companies/uoms")]
public sealed class CompanyUnitsOfMeasureController : ControllerBase
{
    private readonly ICompanyUnitOfMeasureService _companyUnitService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public CompanyUnitsOfMeasureController(
        ICompanyUnitOfMeasureService companyUnitService,
        ICurrentUserProvider currentUserProvider)
    {
        _companyUnitService = companyUnitService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet]
    [RequireCompanyPermission(PermissionKeys.Admin.UsersRead)]
    public async Task<IActionResult> ListEnabled(CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var units = await _companyUnitService.ListEnabledAsync(companyId, cancellationToken);
        var culture = GetRequestedCulture();
        var response = units.Select(unit => new CompanyUnitOfMeasureSummary(
            unit.UnitOfMeasureId,
            unit.UnitOfMeasure?.CanonicalCode ?? string.Empty,
            unit.UnitOfMeasure?.DisplayCode ?? string.Empty,
            unit.Dimension,
            unit.IsEnabled,
            unit.IsDefaultForDimension,
            unit.DisplayNameOverride,
            unit.SortOrder,
            unit.UnitOfMeasure?.Translations.FirstOrDefault(t => string.Equals(t.Culture, culture, StringComparison.OrdinalIgnoreCase))?.Name));

        return Ok(response);
    }

    [HttpPost("{unitOfMeasureId:long}:enable")]
    [RequireCompanyPermission(PermissionKeys.Admin.UsersWrite)]
    public async Task<IActionResult> Enable(
        long unitOfMeasureId,
        [FromBody] CompanyUnitOfMeasureChangeRequest? request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _companyUnitService.EnableAsync(
            companyId,
            unitOfMeasureId,
            request?.DisplayNameOverride,
            request?.SortOrder,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }

    [HttpPost("{unitOfMeasureId:long}:disable")]
    [RequireCompanyPermission(PermissionKeys.Admin.UsersWrite)]
    public async Task<IActionResult> Disable(long unitOfMeasureId, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _companyUnitService.DisableAsync(companyId, unitOfMeasureId, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }

    [HttpPost("defaults")]
    [RequireCompanyPermission(PermissionKeys.Admin.UsersWrite)]
    public async Task<IActionResult> SetDefault(
        [FromBody] CompanyUnitOfMeasureDefaultRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _companyUnitService.SetDefaultAsync(
            companyId,
            request.Dimension,
            request.UnitId,
            cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }

    [HttpPut]
    [RequireCompanyPermission(PermissionKeys.Admin.UsersWrite)]
    public async Task<IActionResult> ReplaceEnabled(
        ReplaceCompanyUnitsOfMeasureRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _companyUnitService.ReplaceEnabledAsync(
            companyId,
            request.UnitOfMeasureIds,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }

    private bool TryGetCompanyId(out long companyId)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null)
        {
            companyId = 0;
            return false;
        }

        companyId = currentUser.CompanyId;
        return true;
    }

    private string GetRequestedCulture()
    {
        var acceptLanguage = HttpContext.Request.Headers.AcceptLanguage.ToString();
        if (string.IsNullOrWhiteSpace(acceptLanguage))
        {
            return CultureInfo.CurrentCulture.Name;
        }

        var culture = acceptLanguage.Split(',').FirstOrDefault();
        return string.IsNullOrWhiteSpace(culture) ? CultureInfo.CurrentCulture.Name : culture.Trim();
    }
}
