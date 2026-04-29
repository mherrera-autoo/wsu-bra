using System.Text.Json;
using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.MasterData.Application.Services;

public interface IGeoSeedService
{
    Task<Result> SeedAsync(CancellationToken cancellationToken = default);
}

public sealed class GeoSeedService : IGeoSeedService
{
    private readonly IGeoSeedSource _seedSource;
    private readonly ICountryRepository _countryRepository;
    private readonly ISubdivisionRepository _subdivisionRepository;
    private readonly ICityRepository _cityRepository;
    private readonly ILocalityRepository _localityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GeoSeedService(
        IGeoSeedSource seedSource,
        ICountryRepository countryRepository,
        ISubdivisionRepository subdivisionRepository,
        ICityRepository cityRepository,
        ILocalityRepository localityRepository,
        IUnitOfWork unitOfWork)
    {
        _seedSource = seedSource;
        _countryRepository = countryRepository;
        _subdivisionRepository = subdivisionRepository;
        _cityRepository = cityRepository;
        _localityRepository = localityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> SeedAsync(CancellationToken cancellationToken = default)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var countries = JsonSerializer.Deserialize<List<CountrySeedDefinition>>(
            await _seedSource.ReadCountriesJsonAsync(cancellationToken),
            options) ?? new List<CountrySeedDefinition>();

        var subdivisions = new List<SubdivisionSeedDefinition>();
        var level1 = JsonSerializer.Deserialize<List<SubdivisionSeedDefinition>>(
            await _seedSource.ReadSubdivisionsLevel1JsonAsync(cancellationToken),
            options);
        if (level1 is { Count: > 0 })
        {
            subdivisions.AddRange(level1);
        }

        var level2 = JsonSerializer.Deserialize<List<SubdivisionSeedDefinition>>(
            await _seedSource.ReadSubdivisionsLevel2JsonAsync(cancellationToken),
            options);
        if (level2 is { Count: > 0 })
        {
            subdivisions.AddRange(level2);
        }

        var cities = JsonSerializer.Deserialize<List<CitySeedDefinition>>(
            await _seedSource.ReadCitiesJsonAsync(cancellationToken),
            options) ?? new List<CitySeedDefinition>();

        var localities = JsonSerializer.Deserialize<List<LocalitySeedDefinition>>(
            await _seedSource.ReadLocalitiesJsonAsync(cancellationToken),
            options) ?? new List<LocalitySeedDefinition>();

        var countriesByIso2 = new Dictionary<string, Country>(StringComparer.OrdinalIgnoreCase);

        foreach (var seed in countries)
        {
            var existing = await _countryRepository.GetByIso2Async(seed.Iso2, cancellationToken);
            if (existing is null)
            {
                var created = Country.Create(
                    seed.Iso2,
                    seed.Iso3,
                    seed.Name,
                    seed.NumericCode,
                    seed.PhonePrefix,
                    seed.CurrencyCode,
                    seed.IsActive);
                await _countryRepository.AddAsync(created, cancellationToken);
                countriesByIso2[created.Iso2] = created;
            }
            else
            {
                existing.Update(seed.Name, seed.NumericCode, seed.PhonePrefix, seed.CurrencyCode, seed.IsActive);
                countriesByIso2[existing.Iso2] = existing;
            }
        }

        var saveResult = await SaveChangesSafelyAsync(cancellationToken);
        if (!saveResult.Success)
        {
            return saveResult;
        }

        var subdivisionsByCode = new Dictionary<string, Subdivision>(StringComparer.OrdinalIgnoreCase);
        foreach (var seed in subdivisions.OrderBy(s => s.Level))
        {
            if (!countriesByIso2.TryGetValue(seed.CountryIso2, out var country))
            {
                return Result.Fail($"Country '{seed.CountryIso2}' not found for subdivision '{seed.Code}'.");
            }

            long? parentId = null;
            if (!string.IsNullOrWhiteSpace(seed.ParentCode))
            {
                var parentKey = BuildSubdivisionKey(country.Id, seed.ParentCode);
                if (!subdivisionsByCode.TryGetValue(parentKey, out var parent))
                {
                    parent = await _subdivisionRepository.GetByCodeAsync(country.Id, seed.ParentCode, cancellationToken);
                    if (parent is null)
                    {
                        return Result.Fail($"Parent subdivision '{seed.ParentCode}' not found for '{seed.Code}'.");
                    }

                    subdivisionsByCode[parentKey] = parent;
                }

                if (parent.Id == 0)
                {
                    var parentSaveResult = await SaveChangesSafelyAsync(cancellationToken);
                    if (!parentSaveResult.Success)
                    {
                        return parentSaveResult;
                    }
                }

                parentId = parent.Id;
            }

            var existing = await _subdivisionRepository.GetByCodeAsync(country.Id, seed.Code, cancellationToken);
            if (existing is null)
            {
                var created = Subdivision.Create(country.Id, seed.Code, seed.Name, seed.Level, parentId, seed.IsActive);
                await _subdivisionRepository.AddAsync(created, cancellationToken);
                subdivisionsByCode[BuildSubdivisionKey(country.Id, created.Code)] = created;
            }
            else
            {
                existing.Update(seed.Name, seed.Level, parentId, seed.IsActive);
                subdivisionsByCode[BuildSubdivisionKey(country.Id, existing.Code)] = existing;
            }
        }

        saveResult = await SaveChangesSafelyAsync(cancellationToken);
        if (!saveResult.Success)
        {
            return saveResult;
        }

        foreach (var seed in cities)
        {
            if (!countriesByIso2.TryGetValue(seed.CountryIso2, out var country))
            {
                return Result.Fail($"Country '{seed.CountryIso2}' not found for city '{seed.Name}'.");
            }

            long? subdivisionId = null;
            if (!string.IsNullOrWhiteSpace(seed.SubdivisionCode))
            {
                var key = BuildSubdivisionKey(country.Id, seed.SubdivisionCode);
                if (!subdivisionsByCode.TryGetValue(key, out var subdivision))
                {
                    subdivision = await _subdivisionRepository.GetByCodeAsync(country.Id, seed.SubdivisionCode, cancellationToken);
                    if (subdivision is null)
                    {
                        return Result.Fail($"Subdivision '{seed.SubdivisionCode}' not found for city '{seed.Name}'.");
                    }

                    subdivisionsByCode[key] = subdivision;
                }

                subdivisionId = subdivision.Id;
            }

            City? existing = null;
            if (!string.IsNullOrWhiteSpace(seed.OfficialCode))
            {
                existing = await _cityRepository.GetByOfficialCodeAsync(country.Id, seed.OfficialCode!, cancellationToken);
            }

            existing ??= await _cityRepository.GetByNameAsync(country.Id, subdivisionId, seed.Name, cancellationToken);

            if (existing is null)
            {
                var created = City.Create(
                    country.Id,
                    subdivisionId,
                    seed.Name,
                    seed.OfficialCode,
                    seed.PostalCode,
                    seed.IsCapital,
                    seed.IsActive);
                await _cityRepository.AddAsync(created, cancellationToken);
            }
            else
            {
                existing.Update(
                    subdivisionId,
                    seed.Name,
                    seed.OfficialCode,
                    seed.PostalCode,
                    seed.IsCapital,
                    seed.IsActive);
            }
        }

        saveResult = await SaveChangesSafelyAsync(cancellationToken);
        if (!saveResult.Success)
        {
            return saveResult;
        }

        foreach (var seed in localities)
        {
            var city = await FindCityAsync(seed, countriesByIso2, subdivisionsByCode, cancellationToken);
            if (city is null)
            {
                return Result.Fail($"City not found for locality '{seed.Name}'.");
            }

            var existing = await _localityRepository.GetByNameAsync(city.Id, seed.Name, cancellationToken);
            if (existing is null)
            {
                var created = Locality.Create(city.Id, seed.Name, seed.IsActive);
                await _localityRepository.AddAsync(created, cancellationToken);
            }
            else
            {
                existing.Update(seed.Name, seed.IsActive);
            }
        }

        return await SaveChangesSafelyAsync(cancellationToken);
    }

    private async Task<City?> FindCityAsync(
        LocalitySeedDefinition seed,
        IReadOnlyDictionary<string, Country> countriesByIso2,
        IDictionary<string, Subdivision> subdivisionsByCode,
        CancellationToken cancellationToken)
    {
        if (!countriesByIso2.TryGetValue(seed.CountryIso2, out var country))
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(seed.CityOfficialCode))
        {
            return await _cityRepository.GetByOfficialCodeAsync(country.Id, seed.CityOfficialCode!, cancellationToken);
        }

        long? subdivisionId = null;
        if (!string.IsNullOrWhiteSpace(seed.SubdivisionCode))
        {
            var key = BuildSubdivisionKey(country.Id, seed.SubdivisionCode);
            if (!subdivisionsByCode.TryGetValue(key, out var subdivision))
            {
                subdivision = await _subdivisionRepository.GetByCodeAsync(country.Id, seed.SubdivisionCode, cancellationToken);
                if (subdivision is null)
                {
                    return null;
                }

                subdivisionsByCode[key] = subdivision;
            }

            subdivisionId = subdivision.Id;
        }

        if (string.IsNullOrWhiteSpace(seed.CityName))
        {
            return null;
        }

        return await _cityRepository.GetByNameAsync(country.Id, subdivisionId, seed.CityName!, cancellationToken);
    }

    private static string BuildSubdivisionKey(long countryId, string code)
        => $"{countryId}:{code.Trim().ToUpperInvariant()}";

    private static string BuildSaveChangesError(Exception exception)
    {
        var message = exception.InnerException?.Message ?? exception.Message;
        return $"Error al guardar cambios de geo seed: {message}";
    }

    private async Task<Result> SaveChangesSafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(BuildSaveChangesError(ex));
        }
    }

    private sealed record CountrySeedDefinition(
        string Iso2,
        string Iso3,
        string Name,
        string? NumericCode,
        string? PhonePrefix,
        string? CurrencyCode,
        bool IsActive = true);

    private sealed record SubdivisionSeedDefinition(
        string CountryIso2,
        string Code,
        string Name,
        short Level,
        string? ParentCode,
        bool IsActive = true);

    private sealed record CitySeedDefinition(
        string CountryIso2,
        string Name,
        string? SubdivisionCode,
        string? OfficialCode,
        string? PostalCode,
        bool IsCapital,
        bool IsActive = true);

    private sealed record LocalitySeedDefinition(
        string CountryIso2,
        string Name,
        string? CityOfficialCode,
        string? CityName,
        string? SubdivisionCode,
        bool IsActive = true);
}
