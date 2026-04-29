using ERP.Modules.Identity.Domain;

namespace ERP.Modules.Identity.Application.Repositories;

public interface ICompanyLinkRepository
{
    Task<CompanyLink?> GetAsync(long organizationId, Guid companyPublicId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(long organizationId, Guid companyPublicId, CancellationToken cancellationToken = default);
    Task AddAsync(CompanyLink link, CancellationToken cancellationToken = default);
}
