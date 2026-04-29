using ERP.Modules.Identity.Application.Services;
using ERP.Modules.Identity.Domain;
using ERP.Modules.MasterData.Contracts;
using ERP.Shared.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ERP.Persistence.Services;

public sealed class RbacService : IRbacService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);
    private readonly ErpDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IFeatureService _featureService;
    private readonly IMemoryCache _memoryCache;

    public RbacService(
        ErpDbContext dbContext,
        ITenantContext tenantContext,
        IFeatureService featureService,
        IMemoryCache memoryCache)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _featureService = featureService;
        _memoryCache = memoryCache;
    }

    public async Task<bool> HasPermissionAsync(long userId, string permissionCode, CancellationToken ct = default)
    {
        var permissions = await GetEffectivePermissionsAsync(userId, ct);
        return permissions.Contains(permissionCode, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<bool> HasPermissionAsync(
        long userId, 
        string permissionCode, 
        RoleAssignmentScopeType requiredScope,
        long? organizationId = null, 
        Guid? companyPublicId = null, 
        CancellationToken ct = default)
    {
        // Validate context requirements
        switch (requiredScope)
        {
            case RoleAssignmentScopeType.Company:
                if (!companyPublicId.HasValue)
                    throw new CompanyContextRequiredException();
                break;
            case RoleAssignmentScopeType.Organization:
                if (!organizationId.HasValue)
                    throw new OrganizationContextRequiredException();
                break;
        }

        var permissions = await GetEffectivePermissionsAsync(userId, requiredScope, organizationId, companyPublicId, ct);
        return permissions.Contains(permissionCode, StringComparer.OrdinalIgnoreCase);
    }

public async Task<IReadOnlyCollection<string>> GetEffectivePermissionsAsync(long userId, CancellationToken ct = default)
    {
        // Default to company scope using tenant context
        var companyId = _tenantContext.CompanyId ?? 0;
        var companyPublicId = _tenantContext.CompanyPublicId;
        
        if (companyId <= 0 || userId <= 0 || !companyPublicId.HasValue)
        {
            return Array.Empty<string>();
        }

        return await GetEffectivePermissionsAsync(userId, RoleAssignmentScopeType.Company, null, companyPublicId, ct);
    }

    public async Task<IReadOnlyCollection<string>> GetEffectivePermissionsAsync(
        long userId,
        RoleAssignmentScopeType scope,
        long? organizationId = null,
        Guid? companyPublicId = null,
        CancellationToken ct = default)
    {
        // Generate cache key with scope context
        var cacheKey = GenerateCacheKey(userId, scope, organizationId, companyPublicId);
        if (_memoryCache.TryGetValue(cacheKey, out HashSet<string>? cachedPermissions) && cachedPermissions is not null)
        {
            return cachedPermissions;
        }

        // Build base query for RoleAssignments
        var roleAssignmentsQuery = _dbContext.RoleAssignments
            .AsNoTracking()
            .Where(ra => ra.UserId == userId && ra.Status == RoleAssignmentStatus.Active);

        // Apply strict scope filtering using assignment scope and context.
        switch (scope)
        {
            case RoleAssignmentScopeType.Platform:
                roleAssignmentsQuery = roleAssignmentsQuery.Where(ra => ra.ScopeType == RoleAssignmentScopeType.Platform);
                break;
            case RoleAssignmentScopeType.Organization:
                if (!organizationId.HasValue)
                    return Array.Empty<string>();
                roleAssignmentsQuery = roleAssignmentsQuery.Where(ra => 
                    ra.ScopeType == RoleAssignmentScopeType.Organization && 
                    ra.OrganizationId == organizationId.Value);
                break;
            case RoleAssignmentScopeType.Company:
                if (!companyPublicId.HasValue)
                    return Array.Empty<string>();
                roleAssignmentsQuery = roleAssignmentsQuery.Where(ra => 
                    ra.ScopeType == RoleAssignmentScopeType.Company && 
                    ra.CompanyPublicId == companyPublicId.Value);
                break;
        }

        // Get permissions through RoleAssignments -> Roles -> RolePermissions -> Permissions
        var permissionsQuery = roleAssignmentsQuery
            .SelectMany(ra => ra.Role.RolePermissions.Select(rp => new
            {
                ra.RoleId,
                ra.Role.RequiredFeatureKey,
                PermissionScope = rp.Permission.ScopeType,
                PermissionCode = rp.Permission.Code
            }));

        var rolePermissions = await permissionsQuery.ToListAsync(ct);

        // Apply feature filtering for company scope
        var allowedPermissions = new List<string>();
        if (scope == RoleAssignmentScopeType.Company && companyPublicId.HasValue)
        {
            var companyId = await GetCompanyIdFromPublicId(companyPublicId.Value, ct);
            if (companyId > 0)
            {
                var roleFeatureMap = rolePermissions
                    .Select(entry => new { entry.RoleId, entry.RequiredFeatureKey })
                    .Distinct()
                    .ToList();

                var allowedRoleIds = new HashSet<long>();
                foreach (var role in roleFeatureMap)
                {
                    if (string.IsNullOrWhiteSpace(role.RequiredFeatureKey))
                    {
                        allowedRoleIds.Add(role.RoleId);
                        continue;
                    }

                    if (await _featureService.IsEnabled(companyId, role.RequiredFeatureKey, ct))
                    {
                        allowedRoleIds.Add(role.RoleId);
                    }
                }

                allowedPermissions.AddRange(rolePermissions
                    .Where(entry => allowedRoleIds.Contains(entry.RoleId)
                        && entry.PermissionScope == scope)
                    .Select(entry => entry.PermissionCode));
            }
        }
        else
        {
            // For platform and organization scopes, no feature filtering
            allowedPermissions.AddRange(rolePermissions
                .Where(entry => entry.PermissionScope == scope)
                .Select(entry => entry.PermissionCode));
        }

        var result = allowedPermissions
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        _memoryCache.Set(cacheKey, result, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration
        });

        return result;
    }

    private async Task<long> GetCompanyIdFromPublicId(Guid companyPublicId, CancellationToken ct)
    {
        var company = await _dbContext.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.PublicId == companyPublicId, ct);
        
        return company?.Id ?? 0;
    }

    private string GenerateCacheKey(long userId, RoleAssignmentScopeType scope, long? organizationId, Guid? companyPublicId)
    {
        var keyParts = new List<string> { "rbac", userId.ToString(), scope.ToString() };
        
        if (organizationId.HasValue)
            keyParts.Add($"org_{organizationId.Value}");
        
        if (companyPublicId.HasValue)
            keyParts.Add($"comp_{companyPublicId.Value}");
        
        return string.Join(":", keyParts);
    }
}
