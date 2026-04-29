using ERP.Modules.Identity.Domain;

namespace ERP.Modules.Identity.Application.Repositories;

public interface IOrganizationMemberRepository
{
    Task<OrganizationMember?> GetAsync(long organizationId, long userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(long organizationId, long userId, CancellationToken cancellationToken = default);
    Task AddAsync(OrganizationMember member, CancellationToken cancellationToken = default);
}
