using ERP.Modules.MasterData.Contracts;

namespace ERP.Modules.MasterData.Application.Repositories;

public interface ICompanyRepository
{
    Task<IReadOnlyList<CompanySnapshot>> ListAllAsync(CancellationToken cancellationToken = default);
    Task<CompanySnapshot?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<CompanySnapshot?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);
    Task<long?> GetIdByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);
    Task<CompanySnapshot?> GetByTaxEntityIdAsync(long taxEntityId, CancellationToken cancellationToken = default);
    Task<long?> GetOrganizationIdAsync(long companyId, CancellationToken cancellationToken = default);
    Task<CompanySnapshot> AddAsync(CompanyCreateRequest company, CancellationToken cancellationToken = default);
    Task UpdateTaxEntityIdAsync(long companyId, long taxEntityId, CancellationToken cancellationToken = default);
}
