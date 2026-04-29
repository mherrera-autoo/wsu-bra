using ERP.Shared.Domain;

namespace ERP.Modules.Identity.Domain;

public sealed class UserPasswordHistory : Entity
{
    public long UserId { get; private set; }
    public string PasswordHash { get; private set; } = null!;
    public string PasswordSalt { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }

    private UserPasswordHistory() { }

    public static UserPasswordHistory Create(long userId, string passwordHash, string passwordSalt)
        => new()
        {
            UserId = userId,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            CreatedAtUtc = DateTime.UtcNow
        };
}