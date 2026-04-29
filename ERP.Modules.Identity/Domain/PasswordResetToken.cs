using ERP.Shared.Domain;

namespace ERP.Modules.Identity.Domain;

public sealed class PasswordResetToken : Entity
{
    public long UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? UsedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private PasswordResetToken() { }

    public static PasswordResetToken Create(long userId, string tokenHash, DateTime expiresAtUtc)
        => new()
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAtUtc = expiresAtUtc,
            CreatedAtUtc = DateTime.UtcNow
        };

    public bool IsValid()
    {
        return UsedAtUtc == null && ExpiresAtUtc > DateTime.UtcNow;
    }

    public void MarkAsUsed()
    {
        UsedAtUtc = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}