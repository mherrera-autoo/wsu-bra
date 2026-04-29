namespace ERP.Notifications.Infrastructure.Email;

public interface IGoogleOAuthTokenService
{
    Task<string> GetAccessTokenAsync(string clientId, string clientSecret, string refreshToken, CancellationToken ct);
}