using ERP.Shared.Domain;

namespace ERP.Modules.Users.Domain;

public sealed class UserProfile : Entity
{
    public long UserId { get; private set; }
    public string Email { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string? DisplayName { get; private set; }
    public bool IsActive { get; private set; } = true;

    private UserProfile() { }

    public static UserProfile Create(long userId, string email, string firstName, string lastName, string? displayName)
        => new()
        {
            UserId = userId,
            Email = NormalizeEmail(email),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            DisplayName = displayName?.Trim(),
            IsActive = true
        };

    public void Update(string email, string firstName, string lastName, string? displayName, bool isActive)
    {
        Email = NormalizeEmail(email);
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        DisplayName = displayName?.Trim();
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        var normalized = email.Trim().ToLowerInvariant();
        if (!normalized.Contains('@', StringComparison.Ordinal))
        {
            throw new ArgumentException("Email must contain '@'.", nameof(email));
        }

        return normalized;
    }
}
