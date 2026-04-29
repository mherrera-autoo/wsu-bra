using ERP.Shared.Domain;

namespace ERP.Modules.Identity.Domain;

public sealed class User : Entity
{
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string PasswordSalt { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;
    public ICollection<CompanyUser> CompanyUsers { get; private set; } = new List<CompanyUser>();

    private User() { }

    public static User Create(string email, string passwordHash, string passwordSalt)
        => new()
        {
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            IsActive = true
        };

    public void UpdatePassword(string passwordHash, string passwordSalt)
    {
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateEmail(string email)
    {
        Email = email.Trim().ToLowerInvariant();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
