namespace ERP.Modules.Identity.Contracts;

public sealed record LoginResult(
    string AccessToken,
    DateTime ExpiresAtUtc,
    AuthUserInfo User);
