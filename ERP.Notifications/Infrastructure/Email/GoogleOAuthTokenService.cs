using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ERP.Notifications.Contracts.Notifications;

namespace ERP.Notifications.Infrastructure.Email;

internal sealed class GoogleOAuthTokenService : IGoogleOAuthTokenService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<GoogleOAuthTokenService> _logger;

    public GoogleOAuthTokenService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        ILogger<GoogleOAuthTokenService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<string> GetAccessTokenAsync(string clientId, string clientSecret, string refreshToken, CancellationToken ct)
    {
        var cacheKey = $"google_oauth_token_{clientId}_{refreshToken.GetHashCode()}";

        if (_memoryCache.TryGetValue(cacheKey, out string? cachedToken) && !string.IsNullOrEmpty(cachedToken))
        {
            _logger.LogDebug("Access token served from cache for client {ClientId}", clientId);
            return cachedToken;
        }

        _logger.LogInformation("Requesting new access token from Google OAuth2 for client {ClientId}", clientId);

        try
        {
            var client = _httpClientFactory.CreateClient("GoogleOAuthToken");
            
            var parameters = new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["refresh_token"] = refreshToken,
                ["grant_type"] = "refresh_token"
            };

            var content = new FormUrlEncodedContent(parameters);
            
            var response = await client.PostAsync("https://oauth2.googleapis.com/token", content, ct);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(ct);
            var tokenResponse = JsonSerializer.Deserialize<GoogleOAuthTokenResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (string.IsNullOrEmpty(tokenResponse?.AccessToken))
            {
                throw new InvalidOperationException("Invalid token response from Google OAuth2");
            }

            var cacheTtl = TimeSpan.FromSeconds(Math.Max(tokenResponse.ExpiresIn - 60, 30));
            _memoryCache.Set(cacheKey, tokenResponse.AccessToken, cacheTtl);

            _logger.LogInformation("Successfully obtained and cached access token for client {ClientId}, expires in {ExpiresIn}s", clientId, tokenResponse.ExpiresIn);

            return tokenResponse.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to obtain access token from Google OAuth2 for client {ClientId}", clientId);
            throw;
        }
    }
}