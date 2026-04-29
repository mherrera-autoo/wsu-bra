using System.Reflection;
using Microsoft.Extensions.Options;

namespace ERP.Modules.MasterData.Application.Services;

public interface IGeoSeedSource
{
    Task<string> ReadCountriesJsonAsync(CancellationToken cancellationToken = default);
    Task<string> ReadSubdivisionsLevel1JsonAsync(CancellationToken cancellationToken = default);
    Task<string> ReadSubdivisionsLevel2JsonAsync(CancellationToken cancellationToken = default);
    Task<string> ReadCitiesJsonAsync(CancellationToken cancellationToken = default);
    Task<string> ReadLocalitiesJsonAsync(CancellationToken cancellationToken = default);
}

public sealed class EmbeddedGeoSeedSource : IGeoSeedSource
{
    private readonly Assembly _assembly;
    private readonly GeoSeedOptions _options;

    public EmbeddedGeoSeedSource(IOptions<GeoSeedOptions> options)
        : this(options, typeof(EmbeddedGeoSeedSource).Assembly)
    {
    }

    internal EmbeddedGeoSeedSource(IOptions<GeoSeedOptions> options, Assembly assembly)
    {
        _options = options.Value ?? throw new InvalidOperationException("Geo seed options are missing.");
        _assembly = assembly;
        EnsureOptionsAreValid(_options);
    }

    internal EmbeddedGeoSeedSource(GeoSeedOptions options, Assembly assembly)
    {
        _options = options ?? throw new InvalidOperationException("Geo seed options are missing.");
        _assembly = assembly;
        EnsureOptionsAreValid(_options);
    }

    public Task<string> ReadCountriesJsonAsync(CancellationToken cancellationToken = default)
        => ReadResourceAsync(_options.CountriesResource, cancellationToken);

    public Task<string> ReadSubdivisionsLevel1JsonAsync(CancellationToken cancellationToken = default)
        => ReadResourceAsync(_options.SubdivisionsLevel1Resource, cancellationToken);

    public Task<string> ReadSubdivisionsLevel2JsonAsync(CancellationToken cancellationToken = default)
        => ReadResourceAsync(_options.SubdivisionsLevel2Resource, cancellationToken);

    public Task<string> ReadCitiesJsonAsync(CancellationToken cancellationToken = default)
        => ReadResourceAsync(_options.CitiesResource, cancellationToken);

    public Task<string> ReadLocalitiesJsonAsync(CancellationToken cancellationToken = default)
        => ReadResourceAsync(_options.LocalitiesResource, cancellationToken);

    private static void EnsureOptionsAreValid(GeoSeedOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.CountriesResource)
            || string.IsNullOrWhiteSpace(options.SubdivisionsLevel1Resource)
            || string.IsNullOrWhiteSpace(options.SubdivisionsLevel2Resource)
            || string.IsNullOrWhiteSpace(options.CitiesResource)
            || string.IsNullOrWhiteSpace(options.LocalitiesResource))
        {
            throw new InvalidOperationException("Geo seed resources must be configured.");
        }
    }

    private async Task<string> ReadResourceAsync(string resourceName, CancellationToken cancellationToken)
    {
        await using var stream = _assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Geo seed resource '{resourceName}' not found.");
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync(cancellationToken);
        return content;
    }
}
