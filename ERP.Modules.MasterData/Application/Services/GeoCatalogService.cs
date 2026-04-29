using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.Application.Services;

public interface IGeoCatalogService
{
    Task<IReadOnlyList<Country>> ListCountriesAsync(bool activeOnly, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Subdivision>> ListSubdivisionsAsync(
        string countryIso2,
        short? level,
        long? parentSubdivisionId,
        bool activeOnly,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<City>> ListCitiesAsync(
        string countryIso2,
        long? subdivisionId,
        string? search,
        bool activeOnly,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Locality>> ListLocalitiesAsync(long cityId, bool activeOnly, CancellationToken cancellationToken = default);
}

public sealed class GeoCatalogService : IGeoCatalogService
{
    private readonly ICountryRepository _countryRepository;
    private readonly ISubdivisionRepository _subdivisionRepository;
    private readonly ICityRepository _cityRepository;
    private readonly ILocalityRepository _localityRepository;

    public GeoCatalogService(
        ICountryRepository countryRepository,
        ISubdivisionRepository subdivisionRepository,
        ICityRepository cityRepository,
        ILocalityRepository localityRepository)
    {
        _countryRepository = countryRepository;
        _subdivisionRepository = subdivisionRepository;
        _cityRepository = cityRepository;
        _localityRepository = localityRepository;
    }

    public Task<IReadOnlyList<Country>> ListCountriesAsync(bool activeOnly, CancellationToken cancellationToken = default)
        => _countryRepository.ListAsync(activeOnly, cancellationToken);

    public async Task<IReadOnlyList<Subdivision>> ListSubdivisionsAsync(
        string countryIso2,
        short? level,
        long? parentSubdivisionId,
        bool activeOnly,
        CancellationToken cancellationToken = default)
    {
        var country = await _countryRepository.GetByIso2Async(countryIso2, cancellationToken);
        if (country is null)
        {
            return Array.Empty<Subdivision>();
        }

        return await _subdivisionRepository.ListAsync(country.Id, level, parentSubdivisionId, activeOnly, cancellationToken);
    }

    public async Task<IReadOnlyList<City>> ListCitiesAsync(
        string countryIso2,
        long? subdivisionId,
        string? search,
        bool activeOnly,
        CancellationToken cancellationToken = default)
    {
        var country = await _countryRepository.GetByIso2Async(countryIso2, cancellationToken);
        if (country is null)
        {
            return Array.Empty<City>();
        }

        return await _cityRepository.ListAsync(country.Id, subdivisionId, search, activeOnly, cancellationToken);
    }

    public Task<IReadOnlyList<Locality>> ListLocalitiesAsync(long cityId, bool activeOnly, CancellationToken cancellationToken = default)
        => _localityRepository.ListAsync(cityId, activeOnly, cancellationToken);
}
