using ERP.EdgePublicApi.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.EdgePublicApi.Integration.Tests;

public sealed class EdgePublicApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseFile;

    public EdgePublicApiWebApplicationFactory(string databaseName)
    {
        _databaseFile = Path.Combine(Path.GetTempPath(), $"{databaseName}.db");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:EdgeTesting"] = $"Data Source={_databaseFile}"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.PostConfigure<EdgePublicApiOptions>(options =>
            {
                options.ApiKey = "test-edge-api-key";
                options.Nodes = new Dictionary<string, EdgeNodeOptions>(StringComparer.OrdinalIgnoreCase)
                {
                    ["EDGE-PLANTA-01"] = new EdgeNodeOptions
                    {
                        CompanyPublicId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        WarehousePublicId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        AllowedZones = ["ZonaLobby", "ZonaBodega"],
                        MovementPolicy = "LobbyToBodega",
                        ReaderIds = ["RFID-GATE-01"],
                        ValidFromUtc = DateTimeOffset.Parse("2026-01-01T00:00:00Z"),
                        ValidToUtc = DateTimeOffset.Parse("2027-01-01T00:00:00Z")
                    }
                };
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}
