using System;
using System.Collections.Generic;

namespace ERP.Api.Contracts.Identity;

public sealed record UpsertPlatformUserRolesRequest(
    Guid UserPublicId,
    IReadOnlyCollection<Guid>? AddRolePublicIds,
    IReadOnlyCollection<Guid>? RemoveRolePublicIds);
