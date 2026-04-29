using System;
using System.Text.Json;
using Xunit;

namespace ERP.Api.Integration.Tests;

public sealed class SwaggerErrorResponsesTests
{
    [Fact]
    public async Task Swagger_Documents_GlobalErrorResponses()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);

        var responses = document.RootElement
            .GetProperty("paths")
            .GetProperty("/api/identity/me")
            .GetProperty("get")
            .GetProperty("responses");

        AssertErrorResponseSchema(responses, "401");
        AssertErrorResponseSchema(responses, "403");
        AssertErrorResponseSchema(responses, "500");
    }

    private static void AssertErrorResponseSchema(JsonElement responses, string statusCode)
    {
        Assert.True(responses.TryGetProperty(statusCode, out var response));

        var schemaRef = response
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema")
            .GetProperty("$ref")
            .GetString();

        Assert.Equal("#/components/schemas/ErrorResponse", schemaRef);
    }
}
