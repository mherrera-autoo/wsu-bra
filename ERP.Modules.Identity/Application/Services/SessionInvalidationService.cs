using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Domain;

namespace ERP.Modules.Identity.Application.Services;

public sealed class SessionInvalidationService(IUserSessionRepository userSessionRepository) : ISessionInvalidationService
{
    public async Task InvalidateUserSessionsAsync(long userId, CancellationToken cancellationToken = default)
    {
        var sessions = await userSessionRepository.GetActiveSessionsByUserAsync(userId, cancellationToken);
        
        foreach (var session in sessions)
        {
            await userSessionRepository.RevokeByIdAsync(session.Id, cancellationToken);
        }
    }
}