using Microsoft.AspNetCore.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequireCompanyMembershipAttribute : AuthorizeAttribute
{
    public const string POLICY_PREFIX = "CompanyMembership";

    public RequireCompanyMembershipAttribute()
    {
        Policy = POLICY_PREFIX;
    }

    public RequireCompanyMembershipAttribute(string permissionCode) : this()
    {
        PermissionCode = permissionCode;
        Policy = $"{POLICY_PREFIX}:{permissionCode}";
    }

    public string? PermissionCode
    {
        get
        {
            var policyParts = Policy?.Split(':');
            return policyParts?.Length > 1 ? policyParts[1] : null;
        }
        set
        {
            Policy = value != null ? $"{POLICY_PREFIX}:{value}" : POLICY_PREFIX;
        }
    }
}