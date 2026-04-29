using ERP.Modules.Identity.Application.Security;
using Microsoft.Extensions.Caching.Memory;

namespace ERP.Api.Authentication;

public sealed class InMemoryIdentityEndpointRateLimiter : IIdentityEndpointRateLimiter
{
    private static readonly TimeSpan LoginWindow = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan RefreshWindow = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan LogoutWindow = TimeSpan.FromMinutes(1);

    private const int LoginLimit = 8;
    private const int RefreshLimit = 45;
    private const int LogoutLimit = 20;

    private readonly IMemoryCache _cache;
    private readonly object _sync = new();

    public InMemoryIdentityEndpointRateLimiter(IMemoryCache cache)
    {
        _cache = cache;
    }

    public bool TryConsumeLogin(string ipAddress, string userKey, out TimeSpan retryAfter)
    {
        var key = $"auth:login:{Normalize(ipAddress)}:{Normalize(userKey)}";
        return TryConsume(key, LoginLimit, LoginWindow, out retryAfter);
    }

    public bool TryConsumeRefresh(Guid sessionId, out TimeSpan retryAfter)
    {
        var key = $"auth:refresh:{sessionId:D}";
        return TryConsume(key, RefreshLimit, RefreshWindow, out retryAfter);
    }

    public bool TryConsumeLogout(string ipAddress, out TimeSpan retryAfter)
    {
        var key = $"auth:logout:{Normalize(ipAddress)}";
        return TryConsume(key, LogoutLimit, LogoutWindow, out retryAfter);
    }

    private bool TryConsume(string key, int limit, TimeSpan window, out TimeSpan retryAfter)
    {
        var now = DateTimeOffset.UtcNow;

        lock (_sync)
        {
            if (!_cache.TryGetValue<RateLimitState>(key, out var state) || state is null || state.WindowEndsAt <= now)
            {
                state = new RateLimitState(0, now.Add(window));
            }

            if (state.Count >= limit)
            {
                retryAfter = state.WindowEndsAt - now;
                if (retryAfter < TimeSpan.Zero)
                {
                    retryAfter = TimeSpan.Zero;
                }

                _cache.Set(key, state, state.WindowEndsAt);
                return false;
            }

            state = state with { Count = state.Count + 1 };
            _cache.Set(key, state, state.WindowEndsAt);
        }

        retryAfter = TimeSpan.Zero;
        return true;
    }

    private static string Normalize(string value)
        => string.IsNullOrWhiteSpace(value) ? "unknown" : value.Trim().ToLowerInvariant();

    private sealed record RateLimitState(int Count, DateTimeOffset WindowEndsAt);
}
