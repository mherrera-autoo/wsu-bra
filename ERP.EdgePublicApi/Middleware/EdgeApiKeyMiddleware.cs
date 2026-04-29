using ERP.EdgePublicApi.Configuration;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace ERP.EdgePublicApi.Middleware;

public sealed class EdgeApiKeyMiddleware
{
    private const string ApiKeyHeaderName = "x-api-key";
    private readonly RequestDelegate _next;
    private readonly ILogger<EdgeApiKeyMiddleware> _logger;

    public EdgeApiKeyMiddleware(RequestDelegate next, ILogger<EdgeApiKeyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IOptionsMonitor<EdgePublicApiOptions> optionsMonitor)
    {
        if (!context.Request.Path.StartsWithSegments("/api/v1/edge", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var configuredApiKey = optionsMonitor.CurrentValue.ApiKey;
        if (string.IsNullOrWhiteSpace(configuredApiKey))
        {
            _logger.LogError("EDGE_PUBLIC_API_KEY is not configured.");
            await ErrorResponses.WriteAsync(context, StatusCodes.Status500InternalServerError, "edge_api_key_not_configured", "Edge public API key is not configured.");
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedApiKey))
        {
            await ErrorResponses.WriteAsync(context, StatusCodes.Status401Unauthorized, "invalid_api_key", "Missing or invalid API key.");
            return;
        }

        var providedApiKeyBytes = Encoding.UTF8.GetBytes(providedApiKey.ToString());
        var configuredApiKeyBytes = Encoding.UTF8.GetBytes(configuredApiKey);
        if (!CryptographicOperations.FixedTimeEquals(providedApiKeyBytes, configuredApiKeyBytes))
        {
            await ErrorResponses.WriteAsync(context, StatusCodes.Status401Unauthorized, "invalid_api_key", "Missing or invalid API key.");
            return;
        }

        await _next(context);
    }
}
