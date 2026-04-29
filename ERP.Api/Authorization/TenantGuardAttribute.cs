using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class TenantGuardAttribute : AuthorizeAttribute
{
    public const string PolicyName = "Tenant";

    public TenantGuardAttribute()
    {
        Policy = PolicyName;
    }
}
