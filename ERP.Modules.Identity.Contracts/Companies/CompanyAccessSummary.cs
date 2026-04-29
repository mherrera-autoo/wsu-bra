
namespace ERP.Modules.Identity.Contracts;

public sealed record CompanyAccessSummary(
    Guid CompanyPublicId,
    long CompanyId,
    long OrganizationId,
    string Name,
    bool HasAccess,
    CompanyAccessSource SourceFlags,
    CompanyLinkAccessType? AccessType)
{
    public long CompanyOrganizationId => OrganizationId;
}
