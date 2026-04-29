using ERP.Modules.Identity.Domain.Enums;

namespace ERP.Modules.Identity.Domain;

public sealed class RoleAssignment
{
    public long Id { get; private set; }
    
    public long UserId { get; private set; }
    public User User { get; private set; }
    
public long RoleId { get; private set; }
    public Role Role { get; private set; }
    
    public long? OrganizationId { get; private set; }
    
    public Guid? CompanyPublicId { get; private set; }
    
    public RoleAssignmentStatus Status { get; private set; }
    
    public DateTime CreatedAtUtc { get; private set; }
    
    public DateTime UpdatedAtUtc { get; private set; }

    private RoleAssignment() { }

public static RoleAssignment Create(
        long userId,
        long roleId,
        long? organizationId = null,
        Guid? companyPublicId = null)
    {
        return new RoleAssignment
        {
            UserId = userId,
            RoleId = roleId,
            OrganizationId = organizationId,
            CompanyPublicId = companyPublicId,
            Status = RoleAssignmentStatus.Active,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

public static RoleAssignment CreatePlatform(long userId, long roleId)
    {
        return new RoleAssignment
        {
            UserId = userId,
            RoleId = roleId,
            OrganizationId = null,
            CompanyPublicId = null,
            Status = RoleAssignmentStatus.Active,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    public static RoleAssignment CreateOrganization(long userId, long roleId, long organizationId)
    {
        return new RoleAssignment
        {
            UserId = userId,
            RoleId = roleId,
            OrganizationId = organizationId,
            CompanyPublicId = null,
            Status = RoleAssignmentStatus.Active,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    public static RoleAssignment CreateCompany(long userId, long roleId, Guid companyPublicId)
    {
        return new RoleAssignment
        {
            UserId = userId,
            RoleId = roleId,
            OrganizationId = null,
            CompanyPublicId = companyPublicId,
            Status = RoleAssignmentStatus.Active,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    public void Activate()
    {
        Status = RoleAssignmentStatus.Active;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status = RoleAssignmentStatus.Inactive;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}