using ERP.Shared.Domain;

namespace ERP.Modules.Identity.Domain;

public sealed class OrganizationMember : Entity
{
    public long OrganizationId { get; private set; }
    public long UserId { get; private set; }
    public OrganizationRole Role { get; private set; }
    public User User { get; private set; } = null!;

    private OrganizationMember() { }

    public static OrganizationMember Create(long organizationId, long userId, OrganizationRole role)
        => new()
        {
            OrganizationId = organizationId,
            UserId = userId,
            Role = role
        };

    public void UpdateRole(OrganizationRole role)
    {
        Role = role;
        UpdatedAt = DateTime.UtcNow;
    }
}
