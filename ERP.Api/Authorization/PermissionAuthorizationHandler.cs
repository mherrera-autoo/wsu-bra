using ERP.Modules.Identity.Application.Services;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Authorization;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IRbacService _rbacService;
    private readonly ITenantContext _tenantContext;

    public PermissionAuthorizationHandler(IRbacService rbacService, ITenantContext tenantContext)
    {
        _rbacService = rbacService;
        _tenantContext = tenantContext;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        return HandlePermissionAsync(context, requirement);
    }

    private async Task HandlePermissionAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userId = _tenantContext.UserId;
        if (userId <= 0)
        {
            return;
        }

        if (await _rbacService.HasPermissionAsync(userId, requirement.PermissionKey))
        {
            context.Succeed(requirement);
        }
    }
}
