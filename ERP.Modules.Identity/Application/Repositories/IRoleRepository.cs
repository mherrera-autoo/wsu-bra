using ERP.Modules.Identity.Domain;

namespace ERP.Modules.Identity.Application.Repositories;

public interface IRoleRepository
{
    Task AddAsync(Role role, CancellationToken cancellationToken = default);
    Task<Role?> GetByIdAsync(long roleId, CancellationToken cancellationToken = default);
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Role?> GetByIdWithPermissionsAsync(RoleAssignmentScopeType scopeType, long roleId, CancellationToken cancellationToken = default);
    Task<Role?> GetByNameAsync(RoleAssignmentScopeType scopeType, string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Role>> ListByPublicIdsAsync(RoleAssignmentScopeType scopeType, IReadOnlyCollection<Guid> rolePublicIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Role>> ListByPublicIdsAsync(IReadOnlyCollection<Guid> rolePublicIds, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(RoleAssignmentScopeType scopeType, string name, long? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Role>> ListWithPermissionsAsync(RoleAssignmentScopeType scopeType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Role>> ListAllWithPermissionsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Role>> ListAssignedWithPermissionsAsync(long userId, CancellationToken cancellationToken = default);
    Task<Role?> GetByIdWithPermissionsAsync(Guid companyPublicId, long roleId, CancellationToken cancellationToken = default);
    Task<Role?> GetByNameAsync(Guid companyPublicId, string name, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(Guid companyPublicId, string name, long? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Role>> ListAsync(Guid companyPublicId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Role>> ListWithPermissionsAsync(Guid companyPublicId, CancellationToken cancellationToken = default);
}
