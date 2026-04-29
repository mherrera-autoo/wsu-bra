using Microsoft.AspNetCore.Authorization;
using ERP.Modules.Identity.Domain;
using System;

namespace ERP.Api.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequireCompanyPermissionAttribute : AuthorizeAttribute
{
    public RequireCompanyPermissionAttribute(string permissionCode)
    {
        Policy = $"{ScopedPermissionPolicyProvider.CompanyPolicyPrefix}{permissionCode}";
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequireOrganizationPermissionAttribute : AuthorizeAttribute
{
    public RequireOrganizationPermissionAttribute(string permissionCode)
    {
        Policy = $"{ScopedPermissionPolicyProvider.OrganizationPolicyPrefix}{permissionCode}";
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequirePlatformPermissionAttribute : AuthorizeAttribute
{
    public RequirePlatformPermissionAttribute(string permissionCode)
    {
        Policy = $"{ScopedPermissionPolicyProvider.PlatformPolicyPrefix}{permissionCode}";
    }
}