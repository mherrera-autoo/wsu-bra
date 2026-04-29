using ERP.Shared.Domain;

namespace ERP.Modules.Identity.Domain;

public sealed class CompanyUser : Entity
{
    public Guid CompanyPublicId { get; private set; }
    public long UserId { get; private set; }
    public CompanyUserStatus Status { get; private set; } = CompanyUserStatus.Active;
    public User User { get; private set; } = null!;

    private CompanyUser() { }

    public static CompanyUser Create(Guid companyPublicId, long userId, CompanyUserStatus status = CompanyUserStatus.Active)
        => new()
        {
            CompanyPublicId = companyPublicId,
            UserId = userId,
            Status = status
        };

    public void SetStatus(CompanyUserStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }
}
