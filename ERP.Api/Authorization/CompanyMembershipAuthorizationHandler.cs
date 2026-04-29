using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Application.Services;
using ERP.Modules.Identity.Domain;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;

public sealed class CompanyMembershipRequirement : IAuthorizationRequirement
{
    public string? PermissionCode { get; }

    public CompanyMembershipRequirement(string? permissionCode = null)
    {
        PermissionCode = permissionCode;
    }
}

public sealed class CompanyMembershipAuthorizationHandler : AuthorizationHandler<CompanyMembershipRequirement>
{
    private readonly ICompanyUserRepository _companyUserRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IRbacService _rbacService;

    public CompanyMembershipAuthorizationHandler(
        ICompanyUserRepository companyUserRepository,
        ICurrentUserProvider currentUserProvider,
        IRbacService rbacService)
    {
        _companyUserRepository = companyUserRepository;
        _currentUserProvider = currentUserProvider;
        _rbacService = rbacService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CompanyMembershipRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser == null)
        {
            return;
        }

        // Check if user has active membership in the current company
        if (!currentUser.CompanyPublicId.HasValue)
        {
            throw new CompanyContextRequiredException();
        }

        var isActiveMember = await _companyUserRepository.IsActiveMemberAsync(
            currentUser.CompanyPublicId.Value, 
            currentUser.UserId);

        if (!isActiveMember)
        {
            throw new CompanyMembershipRequiredException();
        }

        // If the requirement includes permission validation, check it
        if (!string.IsNullOrEmpty(requirement.PermissionCode))
        {
            var hasPermission = await _rbacService.HasPermissionAsync(
                currentUser.UserId,
                requirement.PermissionCode,
                RoleAssignmentScopeType.Company,
                organizationId: null,
                companyPublicId: currentUser.CompanyPublicId);

            if (!hasPermission)
            {
                return;
            }
        }

        context.Succeed(requirement);
    }
}