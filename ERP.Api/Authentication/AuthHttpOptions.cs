using Microsoft.AspNetCore.Http;

namespace ERP.Api.Authentication;

public sealed class AuthHttpOptions
{
    public string[] AllowedOrigins { get; set; } = [];
    public bool CookieSecure { get; set; } = false;
    public string SameSite { get; set; } = nameof(SameSiteMode.Lax);

    public SameSiteMode ResolveSameSiteMode()
        => Enum.TryParse<SameSiteMode>(SameSite, ignoreCase: true, out var sameSiteMode)
            ? sameSiteMode
            : SameSiteMode.Lax;

    public CookieSecurePolicy ResolveCookieSecurePolicy()
        => CookieSecure || ResolveSameSiteMode() == SameSiteMode.None
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.SameAsRequest;

    public bool ResolveCookieSecureFlag(HttpRequest request)
    {
        if (ResolveSameSiteMode() == SameSiteMode.None)
        {
            return true;
        }

        return CookieSecure || request.IsHttps;
    }

    public string ResolveAntiforgeryCookieName()
        => ResolveCookieSecurePolicy() == CookieSecurePolicy.Always ? "__Host-af" : "af";
}
