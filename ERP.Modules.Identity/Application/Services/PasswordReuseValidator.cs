using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Application.Services;
using ERP.Modules.Identity.Application.Security;
using ERP.Modules.Identity.Domain;

namespace ERP.Modules.Identity.Application.Services;

public sealed class PasswordReuseValidator(
    IUserPasswordHistoryRepository passwordHistoryRepository,
    IPasswordHasher passwordHasher) : IPasswordReuseValidator
{
    public async Task EnsureNotReusedAsync(long userId, string newPassword, CancellationToken cancellationToken = default)
    {
        var recentPasswords = await passwordHistoryRepository.GetRecentAsync(userId, 5, cancellationToken);
        
        foreach (var passwordHistory in recentPasswords)
        {
            if (passwordHasher.VerifyPassword(newPassword, passwordHistory.PasswordHash, passwordHistory.PasswordSalt))
            {
                throw new InvalidOperationException("No puede reutilizar una de sus contraseñas recientes.");
            }
        }
    }
}