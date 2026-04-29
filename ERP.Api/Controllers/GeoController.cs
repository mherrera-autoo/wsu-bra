using ERP.Api.Authorization;
using ERP.Api.Contracts.Geo;
using ERP.Modules.MasterData.Application.Services;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/geo")]
[RequirePlatformPermission(PermissionKeys.Platform.GlobalGEOManager)]
public sealed class GeoController : ControllerBase
{
    private readonly IGeoCatalogService _geoCatalogService;

    public GeoController(IGeoCatalogService geoCatalogService)
    {
        _geoCatalogService = geoCatalogService;
    }

    [HttpGet("countries")]
    public async Task<ActionResult<IReadOnlyList<CountryOption>>> ListCountries(
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var countries = await _geoCatalogService.ListCountriesAsync(activeOnly, cancellationToken);
        var response = countries.Select(country => new CountryOption(country.Id, country.Iso2, country.Name));
        return Ok(response);
    }

    [HttpGet("subdivisions")]
    public async Task<ActionResult<IReadOnlyList<SubdivisionOption>>> ListSubdivisions(
        [FromQuery] string? countryIso2,
        [FromQuery] short? level,
        [FromQuery] long? parentId,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(countryIso2))
        {
            return BadRequest(new { error = "countryIso2 is required." });
        }

        var subdivisions = await _geoCatalogService.ListSubdivisionsAsync(
            countryIso2,
            level,
            parentId,
            activeOnly,
            cancellationToken);
        var response = subdivisions.Select(subdivision => new SubdivisionOption(
            subdivision.Id,
            subdivision.Code,
            subdivision.Name,
            subdivision.Level,
            subdivision.ParentSubdivisionId));
        return Ok(response);
    }

    [HttpGet("cities")]
    public async Task<ActionResult<IReadOnlyList<CityOption>>> ListCities(
        [FromQuery] string? countryIso2,
        [FromQuery] long? subdivisionId,
        [FromQuery] string? q,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(countryIso2))
        {
            return BadRequest(new { error = "countryIso2 is required." });
        }

        var cities = await _geoCatalogService.ListCitiesAsync(countryIso2, subdivisionId, q, activeOnly, cancellationToken);
        var response = cities.Select(city => new CityOption(city.Id, city.Name, city.OfficialCode, city.SubdivisionId));
        return Ok(response);
    }

    [HttpGet("localities")]
    public async Task<ActionResult<IReadOnlyList<LocalityOption>>> ListLocalities(
        [FromQuery] long? cityId,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        if (!cityId.HasValue)
        {
            return BadRequest(new { error = "cityId is required." });
        }

        var localities = await _geoCatalogService.ListLocalitiesAsync(cityId.Value, activeOnly, cancellationToken);
        var response = localities.Select(locality => new LocalityOption(locality.Id, locality.Name));
        return Ok(response);
    }
}
