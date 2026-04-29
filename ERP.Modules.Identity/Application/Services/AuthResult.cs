namespace ERP.Modules.Identity.Application.Services;

public sealed record AuthResult(string AccessToken, DateTime ExpiresAtUtc, long SessionId, string? RefreshToken = null);
