using ERP.Modules.Identity.Contracts;

namespace ERP.Modules.Identity.Application.Services;

public sealed class CompanyAccessService
{
    private readonly ICompanyAccessResolver _companyAccessResolver;

    public CompanyAccessService(ICompanyAccessResolver companyAccessResolver)
    {
        _companyAccessResolver = companyAccessResolver;
    }

    public Task<IReadOnlyList<CompanyAccessSummary>> ListForUserAsync(
        long userId,
        long organizationId,
        CompanyListScope scope = CompanyListScope.OperableOnly,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0 || organizationId <= 0)
        {
            return Task.FromResult<IReadOnlyList<CompanyAccessSummary>>(Array.Empty<CompanyAccessSummary>());
        }

        return _companyAccessResolver.ResolveAsync(userId, organizationId, scope, cancellationToken);
    }
}
