namespace ERP.Modules.Identity.Application.Services;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "ERP";
    public string Audience { get; set; } = "ERP";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 30;
    public string RefreshTokenPepper { get; set; } = string.Empty;
    public string RefreshTokenHmacSecret { get; set; } = string.Empty;
}
