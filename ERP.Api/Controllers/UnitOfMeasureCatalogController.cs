using ERP.Api.Authorization;
using ERP.Api.Contracts.MasterData;
using ERP.Modules.MasterData.Application.Services;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/masterdata/uoms")]
public sealed class UnitOfMeasureCatalogController : ControllerBase
{
    private readonly IUnitOfMeasureCatalog _catalog;

    public UnitOfMeasureCatalogController(IUnitOfMeasureCatalog catalog)
    {
        _catalog = catalog;
    }

    [HttpGet]
    [RequirePlatformPermission(PermissionKeys.Admin.UsersRead)]
    public async Task<ActionResult<IReadOnlyList<UnitOfMeasureSummary>>> List(
        [FromQuery(Name = "culture")] string? culture,
        CancellationToken cancellationToken)
    {
        var units = await _catalog.ListAsync(cancellationToken);
        var requestedCulture = string.IsNullOrWhiteSpace(culture) ? null : culture.Trim();

        var response = units.Select(unit =>
        {
            var translation = requestedCulture is null
                ? null
                : unit.Translations.FirstOrDefault(t => string.Equals(t.Culture, requestedCulture, StringComparison.OrdinalIgnoreCase));

            return new UnitOfMeasureSummary(
                unit.Id,
                unit.CanonicalCode,
                unit.DisplayCode,
                unit.Dimension,
                unit.IsBaseUnit,
                unit.FactorToBase,
                unit.PrecisionScale,
                unit.IsActive,
                translation?.Name,
                translation?.Symbol);
        });

        return Ok(response);
    }

    [HttpPost]
    [RequireCompanyPermission(PermissionKeys.Admin.UsersWrite)]
    public async Task<IActionResult> Create(CreateUnitOfMeasureRequest request, CancellationToken cancellationToken)
    {
        var result = await _catalog.CreateAsync(
            request.CanonicalCode,
            request.DisplayCode,
            request.Dimension,
            request.IsBaseUnit,
            request.FactorToBase,
            request.PrecisionScale,
            request.IsActive,
            request.SortOrder,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpPut("{id:long}")]
    [RequireCompanyPermission(PermissionKeys.Admin.UsersWrite)]
    public async Task<IActionResult> Update(long id, UpdateUnitOfMeasureRequest request, CancellationToken cancellationToken)
    {
        var result = await _catalog.UpdateAsync(
            id,
            request.DisplayCode,
            request.Dimension,
            request.IsBaseUnit,
            request.FactorToBase,
            request.PrecisionScale,
            request.IsActive,
            request.SortOrder,
            cancellationToken);

        if (!result.Success)
        {
            if (string.Equals(result.Error, "Unit of measure not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

}
