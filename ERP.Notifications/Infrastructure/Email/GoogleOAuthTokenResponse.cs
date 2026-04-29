using System.Text.Json.Serialization;

namespace ERP.Notifications.Infrastructure.Email;

internal sealed record GoogleOAuthTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = "";

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; init; } = "";

    [JsonPropertyName("error")]
    public string? Error { get; init; }
}