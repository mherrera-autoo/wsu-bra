using ERP.Api.Contracts.Identity;
using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Application.Services;
using ERP.Modules.Identity.Domain;
using ERP.Shared.Application;

namespace ERP.Api.Services;

public sealed class UserPermissionsService
{
    private readonly IRbacService _rbacService;
    private readonly ICompanyUserRepository _companyUserRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IWorkspaceOrganizationResolver _workspaceOrganizationResolver;

    public UserPermissionsService(
        IRbacService rbacService,
        ICompanyUserRepository companyUserRepository,
        ICurrentUserProvider currentUserProvider,
        IWorkspaceOrganizationResolver workspaceOrganizationResolver)
    {
        _rbacService = rbacService;
        _companyUserRepository = companyUserRepository;
        _currentUserProvider = currentUserProvider;
        _workspaceOrganizationResolver = workspaceOrganizationResolver;
    }

    public async Task<UserPermissionsResponse> GetUserPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var platformPermissionsResult = await _rbacService.GetEffectivePermissionsAsync(
            currentUser.UserId, 
            RoleAssignmentScopeType.Platform, 
            null,
            null,
            cancellationToken);

        // Resolve workspace organization instead of using JWT org directly
        var workspaceOrganizationId = await _workspaceOrganizationResolver.ResolveWorkspaceOrganizationIdAsync(
            currentUser.UserId,
            currentUser.OrganizationId > 0 ? currentUser.OrganizationId : null,
            cancellationToken);

        var organizationPermissions = Array.Empty<string>();
        if (workspaceOrganizationId.HasValue)
        {
            var organizationPermissionsResult = await _rbacService.GetEffectivePermissionsAsync(
                currentUser.UserId,
                RoleAssignmentScopeType.Organization,
                workspaceOrganizationId.Value,
                null,
                cancellationToken);
            organizationPermissions = organizationPermissionsResult.ToArray();
        }

        var companyPermissions = Array.Empty<string>();
        var hasCompanyMembership = false;

        if (currentUser.CompanyPublicId.HasValue)
        {
            hasCompanyMembership = await _companyUserRepository.IsActiveMemberAsync(
                currentUser.CompanyPublicId.Value,
                currentUser.UserId,
                cancellationToken);

            if (hasCompanyMembership)
            {
                var companyPermissionsResult = await _rbacService.GetEffectivePermissionsAsync(
                    currentUser.UserId,
                    RoleAssignmentScopeType.Company,
                    null,
                    currentUser.CompanyPublicId.Value,
                    cancellationToken);
                companyPermissions = companyPermissionsResult.ToArray();
            }
        }

        var platformPermissions = platformPermissionsResult.ToArray();

        // Use workspace org ID in context, not JWT org
        var context = new UserPermissionsContext(
            currentUser.CompanyPublicId,
            workspaceOrganizationId,
            hasCompanyMembership);

        return new UserPermissionsResponse(
            platformPermissions.ToArray(),
            organizationPermissions.ToArray(),
            companyPermissions,
            context);
    }
}