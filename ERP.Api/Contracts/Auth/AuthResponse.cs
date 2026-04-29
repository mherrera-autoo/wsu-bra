namespace ERP.Api.Contracts.Auth;

public sealed record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, long SessionId);
