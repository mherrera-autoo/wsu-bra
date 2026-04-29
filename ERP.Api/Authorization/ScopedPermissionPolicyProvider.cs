using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using ERP.Modules.Identity.Domain;
using System;

namespace ERP.Api.Authorization;

public sealed class ScopedPermissionPolicyProvider : IAuthorizationPolicyProvider
{
    public const string CompanyPolicyPrefix = "COMPANY_PERM:";
    public const string OrganizationPolicyPrefix = "ORG_PERM:";
    public const string PlatformPolicyPrefix = "PLATFORM_PERM:";
    public const string LegacyPolicyPrefix = "PERM:"; // Backward compatibility
    
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public ScopedPermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Handle scoped permission policies
        if (policyName.StartsWith(CompanyPolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permissionCode = policyName[CompanyPolicyPrefix.Length..];
            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new ScopedPermissionRequirement(permissionCode, RoleAssignmentScopeType.Company))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        if (policyName.StartsWith(OrganizationPolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permissionCode = policyName[OrganizationPolicyPrefix.Length..];
            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new ScopedPermissionRequirement(permissionCode, RoleAssignmentScopeType.Organization))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        if (policyName.StartsWith(PlatformPolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permissionCode = policyName[PlatformPolicyPrefix.Length..];
            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new ScopedPermissionRequirement(permissionCode, RoleAssignmentScopeType.Platform))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Handle legacy permission policies for backward compatibility
        if (policyName.StartsWith(LegacyPolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permissionKey = policyName[LegacyPolicyPrefix.Length..];
            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(permissionKey))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        => _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        => _fallbackPolicyProvider.GetFallbackPolicyAsync();
}