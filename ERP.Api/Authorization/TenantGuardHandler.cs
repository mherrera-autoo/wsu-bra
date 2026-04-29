using System;
using ERP.Modules.Identity.Application.Security;
using ERP.Modules.MasterData.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Authorization;

public sealed class TenantGuardHandler : AuthorizationHandler<TenantGuardRequirement>
{
    private readonly ICompanyRepository _companyRepository;

    public TenantGuardHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TenantGuardRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var scope = context.User.FindFirst(IdentityClaimTypes.Scope)?.Value;
        if (!string.Equals(scope, "tenant", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var companyIdValue = context.User.FindFirst(IdentityClaimTypes.CompanyId)?.Value;
        var companyPublicIdValue = context.User.FindFirst(IdentityClaimTypes.CompanyPublicId)?.Value;
        if (!long.TryParse(companyIdValue, out var companyId) || companyId <= 0)
        {
            return;
        }

        if (!Guid.TryParse(companyPublicIdValue, out var companyPublicId))
        {
            return;
        }

        var company = await _companyRepository.GetByPublicIdAsync(companyPublicId);
        if (company is null || company.Id != companyId)
        {
            return;
        }

        context.Succeed(requirement);
    }
}
