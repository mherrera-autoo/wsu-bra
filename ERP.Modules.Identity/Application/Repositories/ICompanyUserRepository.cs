using ERP.Modules.Identity.Domain;

namespace ERP.Modules.Identity.Application.Repositories;

public interface ICompanyUserRepository
{
    Task<CompanyUser?> GetAsync(Guid companyPublicId, long userId, CancellationToken cancellationToken = default);
    Task<Guid?> GetActiveCompanyPublicIdAsync(long userId, CancellationToken cancellationToken = default);
    Task<bool> IsActiveMemberAsync(Guid companyPublicId, long userId, CancellationToken cancellationToken = default);
    Task AddAsync(CompanyUser companyUser, CancellationToken cancellationToken = default);
}
