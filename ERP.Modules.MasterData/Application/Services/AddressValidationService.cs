using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.MasterData.Application.Services;

public interface IAddressValidator
{
    Task<Result> ValidateAsync(Address address, CancellationToken cancellationToken = default);
}

public sealed class AddressValidationService : IAddressValidator
{
    private readonly ICountryRepository _countryRepository;
    private readonly ISubdivisionRepository _subdivisionRepository;
    private readonly ICityRepository _cityRepository;
    private readonly ILocalityRepository _localityRepository;

    public AddressValidationService(
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

    public async Task<Result> ValidateAsync(Address address, CancellationToken cancellationToken = default)
    {
        var country = await _countryRepository.GetByIdAsync(address.CountryId, cancellationToken);
        if (country is null)
        {
            return Result.Fail("Country not found.");
        }

        Subdivision? level1 = null;
        if (address.Level1SubdivisionId.HasValue)
        {
            level1 = await _subdivisionRepository.GetByIdAsync(address.Level1SubdivisionId.Value, cancellationToken);
            if (level1 is null)
            {
                return Result.Fail("Level 1 subdivision not found.");
            }

            if (level1.CountryId != address.CountryId)
            {
                return Result.Fail("Level 1 subdivision does not belong to the selected country.");
            }
        }

        Subdivision? level2 = null;
        if (address.Level2SubdivisionId.HasValue)
        {
            level2 = await _subdivisionRepository.GetByIdAsync(address.Level2SubdivisionId.Value, cancellationToken);
            if (level2 is null)
            {
                return Result.Fail("Level 2 subdivision not found.");
            }

            if (level2.CountryId != address.CountryId)
            {
                return Result.Fail("Level 2 subdivision does not belong to the selected country.");
            }

            if (level1 is not null && level2.ParentSubdivisionId != level1.Id)
            {
                return Result.Fail("Level 2 subdivision does not belong to the selected level 1 subdivision.");
            }
        }

        City? city = null;
        if (address.CityId.HasValue)
        {
            city = await _cityRepository.GetByIdAsync(address.CityId.Value, cancellationToken);
            if (city is null)
            {
                return Result.Fail("City not found.");
            }

            if (city.CountryId != address.CountryId)
            {
                return Result.Fail("City does not belong to the selected country.");
            }

            if (level1 is not null && city.SubdivisionId.HasValue)
            {
                var citySubdivision = await _subdivisionRepository.GetByIdAsync(city.SubdivisionId.Value, cancellationToken);
                if (citySubdivision is not null)
                {
                    if (citySubdivision.Level == 2 && citySubdivision.ParentSubdivisionId != level1.Id)
                    {
                        return Result.Fail("City subdivision does not belong to the selected level 1 subdivision.");
                    }

                    if (citySubdivision.Level == 1 && citySubdivision.Id != level1.Id)
                    {
                        return Result.Fail("City subdivision does not match the selected level 1 subdivision.");
                    }
                }
            }

            if (level2 is not null && city.SubdivisionId.HasValue)
            {
                var citySubdivision = await _subdivisionRepository.GetByIdAsync(city.SubdivisionId.Value, cancellationToken);
                if (citySubdivision is not null && citySubdivision.Level == 2 && citySubdivision.Id != level2.Id)
                {
                    return Result.Fail("City subdivision does not match the selected level 2 subdivision.");
                }
            }
        }

        if (address.LocalityId.HasValue)
        {
            var locality = await _localityRepository.GetByIdAsync(address.LocalityId.Value, cancellationToken);
            if (locality is null)
            {
                return Result.Fail("Locality not found.");
            }

            if (!address.CityId.HasValue || locality.CityId != address.CityId.Value)
            {
                return Result.Fail("Locality does not belong to the selected city.");
            }
        }

        return Result.Ok();
    }
}
