namespace ERP.Modules.Identity.Application.Repositories;

public interface ICompanyLookupRepository
{
    Task<long?> GetOrganizationIdAsync(long companyId, CancellationToken cancellationToken = default);
    Task<Guid?> GetCompanyPublicIdAsync(long companyId, CancellationToken cancellationToken = default);
}
