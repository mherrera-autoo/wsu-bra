using ERP.Modules.Identity.Domain;

namespace ERP.Modules.Identity.Application.Repositories;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetValidByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task CreateAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
    Task MarkAsUsedAsync(long tokenId, CancellationToken cancellationToken = default);
}