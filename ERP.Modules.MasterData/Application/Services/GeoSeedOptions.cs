namespace ERP.Modules.MasterData.Application.Services;

public sealed class GeoSeedOptions
{
    public string CountriesResource { get; init; } = string.Empty;
    public string SubdivisionsLevel1Resource { get; init; } = string.Empty;
    public string SubdivisionsLevel2Resource { get; init; } = string.Empty;
    public string CitiesResource { get; init; } = string.Empty;
    public string LocalitiesResource { get; init; } = string.Empty;
}
