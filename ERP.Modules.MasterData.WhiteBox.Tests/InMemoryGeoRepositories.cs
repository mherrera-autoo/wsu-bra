using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.WhiteBox.Tests;

internal sealed class InMemoryCountryRepository : ICountryRepository
{
    private readonly List<Country> _countries = new();

    public Task AddAsync(Country country, CancellationToken cancellationToken = default)
    {
        if (country.Id == 0)
        {
            var nextId = _countries.Count == 0 ? 1 : _countries.Max(c => c.Id) + 1;
            typeof(Country).GetProperty("Id")!.SetValue(country, nextId);
        }

        _countries.Add(country);
        return Task.CompletedTask;
    }

    public Task<Country?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => Task.FromResult(_countries.FirstOrDefault(country => country.Id == id));

    public Task<Country?> GetByIso2Async(string iso2, CancellationToken cancellationToken = default)
    {
        var normalized = iso2.Trim().ToUpperInvariant();
        return Task.FromResult(_countries.FirstOrDefault(country => country.Iso2 == normalized));
    }

    public Task<IReadOnlyList<Country>> ListAsync(bool activeOnly, CancellationToken cancellationToken = default)
    {
        var query = _countries.AsEnumerable();
        if (activeOnly)
        {
            query = query.Where(country => country.IsActive);
        }

        return Task.FromResult<IReadOnlyList<Country>>(query.OrderBy(country => country.Name).ToList());
    }

    public IReadOnlyList<Country> Snapshot() => _countries.ToList();
}

internal sealed class InMemorySubdivisionRepository : ISubdivisionRepository
{
    private readonly List<Subdivision> _subdivisions = new();

    public Task AddAsync(Subdivision subdivision, CancellationToken cancellationToken = default)
    {
        if (subdivision.Id == 0)
        {
            var nextId = _subdivisions.Count == 0 ? 1 : _subdivisions.Max(s => s.Id) + 1;
            typeof(Subdivision).GetProperty("Id")!.SetValue(subdivision, nextId);
        }

        _subdivisions.Add(subdivision);
        return Task.CompletedTask;
    }

    public Task<Subdivision?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => Task.FromResult(_subdivisions.FirstOrDefault(subdivision => subdivision.Id == id));

    public Task<Subdivision?> GetByCodeAsync(long countryId, string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return Task.FromResult(_subdivisions.FirstOrDefault(subdivision =>
            subdivision.CountryId == countryId && subdivision.Code == normalized));
    }

    public Task<IReadOnlyList<Subdivision>> ListAsync(
        long countryId,
        short? level,
        long? parentSubdivisionId,
        bool activeOnly,
        CancellationToken cancellationToken = default)
    {
        var query = _subdivisions.Where(subdivision => subdivision.CountryId == countryId);
        if (level.HasValue)
        {
            query = query.Where(subdivision => subdivision.Level == level);
        }

        if (parentSubdivisionId.HasValue)
        {
            query = query.Where(subdivision => subdivision.ParentSubdivisionId == parentSubdivisionId);
        }

        if (activeOnly)
        {
            query = query.Where(subdivision => subdivision.IsActive);
        }

        return Task.FromResult<IReadOnlyList<Subdivision>>(query.OrderBy(subdivision => subdivision.Name).ToList());
    }

    public IReadOnlyList<Subdivision> Snapshot() => _subdivisions.ToList();
}

internal sealed class InMemoryCityRepository : ICityRepository
{
    private readonly List<City> _cities = new();

    public Task AddAsync(City city, CancellationToken cancellationToken = default)
    {
        if (city.Id == 0)
        {
            var nextId = _cities.Count == 0 ? 1 : _cities.Max(c => c.Id) + 1;
            typeof(City).GetProperty("Id")!.SetValue(city, nextId);
        }

        _cities.Add(city);
        return Task.CompletedTask;
    }

    public Task<City?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => Task.FromResult(_cities.FirstOrDefault(city => city.Id == id));

    public Task<City?> GetByOfficialCodeAsync(long countryId, string officialCode, CancellationToken cancellationToken = default)
    {
        var normalized = officialCode.Trim();
        return Task.FromResult(_cities.FirstOrDefault(city =>
            city.CountryId == countryId && city.OfficialCode == normalized));
    }

    public Task<City?> GetByNameAsync(long countryId, long? subdivisionId, string name, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim();
        return Task.FromResult(_cities.FirstOrDefault(city =>
            city.CountryId == countryId && city.SubdivisionId == subdivisionId && city.Name == normalized));
    }

    public Task<IReadOnlyList<City>> ListAsync(
        long countryId,
        long? subdivisionId,
        string? search,
        bool activeOnly,
        CancellationToken cancellationToken = default)
    {
        var query = _cities.Where(city => city.CountryId == countryId);
        if (subdivisionId.HasValue)
        {
            query = query.Where(city => city.SubdivisionId == subdivisionId);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(city => city.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (activeOnly)
        {
            query = query.Where(city => city.IsActive);
        }

        return Task.FromResult<IReadOnlyList<City>>(query.OrderBy(city => city.Name).ToList());
    }

    public IReadOnlyList<City> Snapshot() => _cities.ToList();
}

internal sealed class InMemoryLocalityRepository : ILocalityRepository
{
    private readonly List<Locality> _localities = new();

    public Task AddAsync(Locality locality, CancellationToken cancellationToken = default)
    {
        if (locality.Id == 0)
        {
            var nextId = _localities.Count == 0 ? 1 : _localities.Max(l => l.Id) + 1;
            typeof(Locality).GetProperty("Id")!.SetValue(locality, nextId);
        }

        _localities.Add(locality);
        return Task.CompletedTask;
    }

    public Task<Locality?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => Task.FromResult(_localities.FirstOrDefault(locality => locality.Id == id));

    public Task<Locality?> GetByNameAsync(long cityId, string name, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim();
        return Task.FromResult(_localities.FirstOrDefault(locality =>
            locality.CityId == cityId && locality.Name == normalized));
    }

    public Task<IReadOnlyList<Locality>> ListAsync(long cityId, bool activeOnly, CancellationToken cancellationToken = default)
    {
        var query = _localities.Where(locality => locality.CityId == cityId);
        if (activeOnly)
        {
            query = query.Where(locality => locality.IsActive);
        }

        return Task.FromResult<IReadOnlyList<Locality>>(query.OrderBy(locality => locality.Name).ToList());
    }

    public IReadOnlyList<Locality> Snapshot() => _localities.ToList();
}
