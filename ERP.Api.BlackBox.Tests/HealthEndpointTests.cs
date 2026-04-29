using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ERP.Api.BlackBox.Tests;

public sealed class HealthEndpointTests
{
    [Fact]
    public async Task Health_ReturnsOkStatus()
    {
        using var factory = new BlackBoxApiWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.NotNull(payload);
        Assert.Equal("ok", payload!.Status);
    }

    private sealed record HealthResponse(string Status);
}
