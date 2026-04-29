using ERP.Modules.Identity.Contracts;
using ERP.Shared.Domain;

namespace ERP.Modules.Identity.Domain;

public sealed class CompanyLink : Entity
{
    public long OrganizationId { get; private set; }
    public Guid CompanyPublicId { get; private set; }
    public CompanyLinkAccessType AccessType { get; private set; }

    private CompanyLink() { }

    public static CompanyLink Create(long organizationId, Guid companyPublicId, CompanyLinkAccessType accessType)
        => new()
        {
            OrganizationId = organizationId,
            CompanyPublicId = companyPublicId,
            AccessType = accessType
        };
}
