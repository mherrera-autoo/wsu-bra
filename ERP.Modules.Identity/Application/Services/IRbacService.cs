using ERP.Modules.Identity.Domain;

namespace ERP.Modules.Identity.Application.Services;

public interface IRbacService
{
    Task<bool> HasPermissionAsync(long userId, string permissionCode, CancellationToken ct = default);
    Task<bool> HasPermissionAsync(long userId, string permissionCode, RoleAssignmentScopeType requiredScope, long? organizationId = null, Guid? companyPublicId = null, CancellationToken ct = default);
    Task<IReadOnlyCollection<string>> GetEffectivePermissionsAsync(long userId, CancellationToken ct = default);
    Task<IReadOnlyCollection<string>> GetEffectivePermissionsAsync(long userId, RoleAssignmentScopeType scope, long? organizationId = null, Guid? companyPublicId = null, CancellationToken ct = default);
}
