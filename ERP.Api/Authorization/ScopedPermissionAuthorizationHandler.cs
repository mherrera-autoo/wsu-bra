using ERP.Modules.Identity.Application.Services;
using ERP.Modules.Identity.Domain;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;

public sealed class ScopedPermissionRequirement : IAuthorizationRequirement
{
    public ScopedPermissionRequirement(string permissionCode, RoleAssignmentScopeType scope)
    {
        PermissionCode = permissionCode;
        Scope = scope;
    }

    public string PermissionCode { get; }
    public RoleAssignmentScopeType Scope { get; }
}

public sealed class ScopedPermissionAuthorizationHandler : AuthorizationHandler<ScopedPermissionRequirement>
{
    private readonly IRbacService _rbacService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserProvider _currentUserProvider;

    public ScopedPermissionAuthorizationHandler(IRbacService rbacService, ITenantContext tenantContext, ICurrentUserProvider currentUserProvider)
    {
        _rbacService = rbacService;
        _tenantContext = tenantContext;
        _currentUserProvider = currentUserProvider;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ScopedPermissionRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        return HandleScopedPermissionAsync(context, requirement);
    }

    private async Task HandleScopedPermissionAsync(
        AuthorizationHandlerContext context,
        ScopedPermissionRequirement requirement)
    {
        var userId = _tenantContext.UserId;
        if (userId <= 0)
        {
            return;
        }

        var hasPermission = await HasScopedPermissionAsync(userId, requirement.PermissionCode, requirement.Scope);
        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }

    private async Task<bool> HasScopedPermissionAsync(long userId, string permissionCode, RoleAssignmentScopeType requiredScope)
    {
        // Get current user for context extraction
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser == null)
        {
            return false;
        }

        // Extract context based on required scope
        var organizationId = requiredScope == RoleAssignmentScopeType.Organization ? currentUser.OrganizationId : (long?)null;
        var companyPublicId = requiredScope == RoleAssignmentScopeType.Company ? currentUser.CompanyPublicId : (Guid?)null;

        try
        {
            return await _rbacService.HasPermissionAsync(userId, permissionCode, requiredScope, organizationId, companyPublicId);
        }
        catch (RbacException)
        {
            // Context validation failed - return false instead of throwing
            return false;
        }
    }
}