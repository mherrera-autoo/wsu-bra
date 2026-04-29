using ERP.Modules.MasterData.Application.Services;
using ERP.Modules.MasterData.Domain;
using Xunit;

namespace ERP.Modules.MasterData.WhiteBox.Tests;

public sealed class AddressValidationServiceTests
{
    [Fact]
    public async Task ValidateAsync_ShouldFailWhenSubdivisionCountryMismatch()
    {
        var (service, context) = BuildService();
        var address = Address.Create(
            context.Chile.Id,
            context.ArgentinaRegion.Id,
            null,
            null,
            null,
            "Main",
            null,
            null,
            null,
            null,
            null,
            null);

        var result = await service.ValidateAsync(address);

        Assert.False(result.Success);
        Assert.Contains("subdivision", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ValidateAsync_ShouldFailWhenLevel2ParentMismatch()
    {
        var (service, context) = BuildService();
        var address = Address.Create(
            context.Chile.Id,
            context.ChileRegion.Id,
            context.MismatchedProvince.Id,
            null,
            null,
            "Main",
            null,
            null,
            null,
            null,
            null,
            null);

        var result = await service.ValidateAsync(address);

        Assert.False(result.Success);
        Assert.Contains("level 2", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ValidateAsync_ShouldFailWhenLocalityCityMismatch()
    {
        var (service, context) = BuildService();
        var address = Address.Create(
            context.Chile.Id,
            context.ChileRegion.Id,
            context.ChileProvince.Id,
            context.CitySantiago.Id,
            context.LocalityValparaiso.Id,
            "Main",
            null,
            null,
            null,
            null,
            null,
            null);

        var result = await service.ValidateAsync(address);

        Assert.False(result.Success);
        Assert.Contains("locality", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    private static (AddressValidationService Service, AddressValidationContext Context) BuildService()
    {
        var countryRepository = new InMemoryCountryRepository();
        var subdivisionRepository = new InMemorySubdivisionRepository();
        var cityRepository = new InMemoryCityRepository();
        var localityRepository = new InMemoryLocalityRepository();

        var chile = Country.Create("CL", "CHL", "Chile", "152", "+56", "CLP", true);
        var argentina = Country.Create("AR", "ARG", "Argentina", "032", "+54", "ARS", true);
        countryRepository.AddAsync(chile).GetAwaiter().GetResult();
        countryRepository.AddAsync(argentina).GetAwaiter().GetResult();

        var chileRegion = Subdivision.Create(chile.Id, "CL-RM", "Región Metropolitana", 1, null, true);
        var argentinaRegion = Subdivision.Create(argentina.Id, "AR-B", "Buenos Aires", 1, null, true);
        subdivisionRepository.AddAsync(chileRegion).GetAwaiter().GetResult();
        subdivisionRepository.AddAsync(argentinaRegion).GetAwaiter().GetResult();

        var chileProvince = Subdivision.Create(chile.Id, "CL-RM-PRO", "Provincia Santiago", 2, chileRegion.Id, true);
        var mismatchedProvince = Subdivision.Create(chile.Id, "CL-RM-OTHER", "Provincia Otra", 2, null, true);
        subdivisionRepository.AddAsync(chileProvince).GetAwaiter().GetResult();
        subdivisionRepository.AddAsync(mismatchedProvince).GetAwaiter().GetResult();

        var citySantiago = City.Create(chile.Id, chileProvince.Id, "Santiago", "13101", null, true, true);
        var cityValparaiso = City.Create(chile.Id, chileRegion.Id, "Valparaíso", "05101", null, true, true);
        cityRepository.AddAsync(citySantiago).GetAwaiter().GetResult();
        cityRepository.AddAsync(cityValparaiso).GetAwaiter().GetResult();

        var localityValparaiso = Locality.Create(cityValparaiso.Id, "Plan", true);
        localityRepository.AddAsync(localityValparaiso).GetAwaiter().GetResult();

        var service = new AddressValidationService(
            countryRepository,
            subdivisionRepository,
            cityRepository,
            localityRepository);

        return (service, new AddressValidationContext(
            chile,
            argentina,
            chileRegion,
            argentinaRegion,
            chileProvince,
            mismatchedProvince,
            citySantiago,
            localityValparaiso));
    }

    private sealed record AddressValidationContext(
        Country Chile,
        Country Argentina,
        Subdivision ChileRegion,
        Subdivision ArgentinaRegion,
        Subdivision ChileProvince,
        Subdivision MismatchedProvince,
        City CitySantiago,
        Locality LocalityValparaiso);
}
