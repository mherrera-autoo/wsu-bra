namespace ERP.Api.Authentication;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "ERP";
    public string Audience { get; set; } = "ERP";
    public string SigningKey { get; set; } = "change-me";
    public int AccessTokenMinutes { get; set; } = 60;
}
