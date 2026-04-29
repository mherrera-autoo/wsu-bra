using ERP.Modules.MasterData.Contracts;

namespace ERP.Modules.Identity.Application.Services;

public interface ICompanyAccessSource
{
    bool Supports(OrganizationType accountType);

    Task<IReadOnlyCollection<CompanyAccessCandidate>> GetCandidatesAsync(
        long userId,
        long organizationId,
        CancellationToken cancellationToken = default);
}
