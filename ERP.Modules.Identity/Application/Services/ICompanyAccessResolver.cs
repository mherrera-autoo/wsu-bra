using ERP.Modules.Identity.Contracts;

namespace ERP.Modules.Identity.Application.Services;

public interface ICompanyAccessResolver
{
    Task<IReadOnlyList<CompanyAccessSummary>> ResolveAsync(
        long userId,
        long organizationId,
        CompanyListScope scope,
        CancellationToken cancellationToken = default);
}
