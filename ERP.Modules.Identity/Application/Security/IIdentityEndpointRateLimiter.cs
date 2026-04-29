namespace ERP.Modules.Identity.Application.Security;

public interface IIdentityEndpointRateLimiter
{
    bool TryConsumeLogin(string ipAddress, string userKey, out TimeSpan retryAfter);
    bool TryConsumeRefresh(Guid sessionId, out TimeSpan retryAfter);
    bool TryConsumeLogout(string ipAddress, out TimeSpan retryAfter);
}
