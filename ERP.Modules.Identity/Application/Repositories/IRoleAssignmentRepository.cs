using ERP.Modules.Identity.Domain;

namespace ERP.Modules.Identity.Application.Repositories;

public interface IRoleAssignmentRepository
{
    Task AddAsync(RoleAssignment roleAssignment, CancellationToken cancellationToken = default);
    Task<RoleAssignment?> GetAsync(long userId, long roleId, RoleAssignmentScopeType scopeType, long? organizationId = null, Guid? companyPublicId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoleAssignment>> GetByUserAsync(long userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoleAssignment>> GetByOrganizationAsync(long organizationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoleAssignment>> GetByCompanyAsync(Guid companyPublicId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(long userId, long roleId, RoleAssignmentScopeType scopeType, long? organizationId = null, Guid? companyPublicId = null, CancellationToken cancellationToken = default);
    Task UpdateAsync(RoleAssignment roleAssignment, CancellationToken cancellationToken = default);
}