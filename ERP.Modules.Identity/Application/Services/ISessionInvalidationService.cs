namespace ERP.Modules.Identity.Application.Services;

public interface ISessionInvalidationService
{
    Task InvalidateUserSessionsAsync(long userId, CancellationToken cancellationToken = default);
}