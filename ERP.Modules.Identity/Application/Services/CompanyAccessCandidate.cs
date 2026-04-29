using ERP.Modules.Identity.Contracts;

namespace ERP.Modules.Identity.Application.Services;

public sealed record CompanyAccessCandidate(
    Guid CompanyPublicId,
    CompanyAccessSource Source,
    CompanyLinkAccessType? AccessType = null);
