using System.Reflection;
using System.Text.Json;
using ERP.Modules.Accounting.Application.Services;

namespace ERP.Persistence.Services;

public sealed class JsonChartOfAccountsTemplateSource : IChartOfAccountsTemplateSource
{
    private readonly Assembly _assembly;
    private readonly JsonSerializerOptions _serializerOptions;

    public JsonChartOfAccountsTemplateSource()
    {
        _assembly = typeof(JsonChartOfAccountsTemplateSource).Assembly;
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<IReadOnlyList<ChartOfAccountsTemplateEntry>> LoadAsync(
        string version,
        CancellationToken cancellationToken = default)
    {
        var resourceName = $"ERP.Persistence.SeedData.accounting.{version}.chart-of-accounts.json";
        await using var stream = _assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return Array.Empty<ChartOfAccountsTemplateEntry>();
        }

        var entries = await JsonSerializer.DeserializeAsync<List<ChartOfAccountsTemplateEntry>>(stream, _serializerOptions, cancellationToken);
        return entries ?? new List<ChartOfAccountsTemplateEntry>();
    }
}
