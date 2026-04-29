using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

public sealed class CompanyMembershipPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public CompanyMembershipPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(RequireCompanyMembershipAttribute.POLICY_PREFIX, StringComparison.OrdinalIgnoreCase))
        {
            var policy = new AuthorizationPolicyBuilder();

            // Extract permission code if present
            var policyParts = policyName.Split(':');
            var permissionCode = policyParts.Length > 1 ? policyParts[1] : null;

            policy.AddRequirements(new CompanyMembershipRequirement(permissionCode));
            return Task.FromResult<AuthorizationPolicy?>(policy.Build());
        }

        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}