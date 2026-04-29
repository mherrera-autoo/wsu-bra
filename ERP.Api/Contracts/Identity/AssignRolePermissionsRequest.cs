using System.Collections.Generic;

namespace ERP.Api.Contracts.Identity;

public sealed record AssignRolePermissionsRequest(IReadOnlyCollection<long> PermissionIds);
