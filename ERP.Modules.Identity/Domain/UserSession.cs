using ERP.Shared.Domain;

namespace ERP.Modules.Identity.Domain;

public sealed class UserSession : Entity
{
    public Guid SessionId { get; private set; }
    public long UserId { get; private set; }
    public string DeviceInfo { get; private set; } = null!;
    public string RefreshTokenHash { get; private set; } = null!;
    public string? ReplacedByTokenHash { get; private set; }
    public DateTime? ReplacedAtUtc { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public DateTime LastSeenAt { get; private set; }
    public User User { get; private set; } = null!;

    private UserSession() { }

    public static UserSession Create(
        long userId,
        string deviceInfo,
        string refreshTokenHash,
        DateTime expiresAt,
        Guid? sessionId = null)
    {
        var now = DateTime.UtcNow;

        return new UserSession
        {
            SessionId = sessionId ?? Guid.NewGuid(),
            UserId = userId,
            DeviceInfo = string.IsNullOrWhiteSpace(deviceInfo) ? "Unknown" : deviceInfo,
            RefreshTokenHash = refreshTokenHash,
            ExpiresAt = expiresAt,
            CreatedAt = now,
            LastSeenAt = now
        };
    }

    public void MarkSeen(DateTime seenAt)
    {
        LastSeenAt = seenAt;
        UpdatedAt = seenAt;
    }

    public void Revoke(DateTime revokedAt)
    {
        RevokedAt ??= revokedAt;
        UpdatedAt = revokedAt;
    }

    public void MarkReplaced(string replacedByTokenHash, DateTime replacedAtUtc)
    {
        ReplacedByTokenHash = replacedByTokenHash;
        ReplacedAtUtc = replacedAtUtc;
        RevokedAt = replacedAtUtc;
        UpdatedAt = replacedAtUtc;
    }
}
