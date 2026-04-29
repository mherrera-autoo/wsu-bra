using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class PasswordResetTokenRepository(ErpDbContext context) : IPasswordResetTokenRepository
{
    public async Task<PasswordResetToken?> GetValidByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return await context.PasswordResetTokens
            .Where(t => t.TokenHash == tokenHash && t.UsedAtUtc == null && t.ExpiresAtUtc > DateTime.UtcNow)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task CreateAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
    {
        await context.PasswordResetTokens.AddAsync(token, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAsUsedAsync(long tokenId, CancellationToken cancellationToken = default)
    {
        var token = await context.PasswordResetTokens.FindAsync([tokenId], cancellationToken);
        if (token != null)
        {
            token.MarkAsUsed();
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}