using ERP.Modules.Identity.Application.Security;
using ERP.Modules.Identity.Application.Services;
using ERP.Modules.Identity.Domain;
using ERP.Modules.MasterData.Domain;
using ERP.Persistence;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.Api.Integration.Tests;

public sealed class TestRbacService : IRbacService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> RolePermissions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [RoleNames.PlatformSuperAdmin] = new[]
            {
                PermissionKeys.Platform.FeaturesManage,
                PermissionKeys.Admin.UsersRead,
                PermissionKeys.Admin.UsersWrite,
                PermissionKeys.Admin.RolesAssign,
                PermissionKeys.Accounting.ChartOfAccountsRead,
                PermissionKeys.Accounting.ChartOfAccountsWrite,
                PermissionKeys.Accounting.JournalRead,
                PermissionKeys.Accounting.JournalAdjustmentsWrite,
                PermissionKeys.Accounting.PeriodClose,
                PermissionKeys.Inventory.StockRead,
                PermissionKeys.Inventory.MovementsCreate,
                PermissionKeys.Inventory.AdjustmentsCreate,
                PermissionKeys.Pharmaceutical.DispenseCreate,
                PermissionKeys.Pharmaceutical.ControlledBookRead
            },
            [RoleNames.FeaturePackAdder] = new[]
            {
                PermissionKeys.Platform.FeaturesManage
            },
            [RoleNames.CompanyAdmin] = new[]
            {
                PermissionKeys.Admin.UsersRead,
                PermissionKeys.Admin.UsersWrite,
                PermissionKeys.Admin.RolesAssign,
                PermissionKeys.Accounting.ChartOfAccountsRead,
                PermissionKeys.Accounting.ChartOfAccountsWrite,
                PermissionKeys.Inventory.StockRead
            },
            [RoleNames.CompanyAccountant] = new[]
            {
                PermissionKeys.Accounting.ChartOfAccountsRead,
                PermissionKeys.Accounting.ChartOfAccountsWrite,
                PermissionKeys.Accounting.JournalRead,
                PermissionKeys.Accounting.JournalAdjustmentsWrite,
                PermissionKeys.Accounting.PeriodClose
            },
            [RoleNames.FinanceClerk] = new[]
            {
                PermissionKeys.Accounting.JournalRead,
                PermissionKeys.Inventory.StockRead
            },
            [RoleNames.WarehouseClerk] = new[]
            {
                PermissionKeys.Inventory.StockRead,
                PermissionKeys.Inventory.MovementsCreate
            },
            [RoleNames.CompanyAuditor] = new[]
            {
                PermissionKeys.Accounting.ChartOfAccountsRead,
                PermissionKeys.Accounting.JournalRead,
                PermissionKeys.Inventory.StockRead
            },
            [RoleNames.Pharmacist] = new[]
            {
                PermissionKeys.Pharmaceutical.DispenseCreate
            },
            [RoleNames.PharmacySupervisor] = new[]
            {
                PermissionKeys.Pharmaceutical.DispenseCreate,
                PermissionKeys.Pharmaceutical.ControlledBookRead
            },
            [RoleNames.PharmacyAuditor] = new[]
            {
                PermissionKeys.Pharmaceutical.ControlledBookRead
            },
            [RoleNames.OrganizationOwner] = new[]
            {
                "Workspace.Companies.Create",
                "Workspace.Manage"
            }
        };

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ErpDbContext _dbContext;

    public TestRbacService(IHttpContextAccessor httpContextAccessor, ErpDbContext dbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
    }

public Task<bool> HasPermissionAsync(long userId, string permissionCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(permissionCode))
        {
            return Task.FromResult(false);
        }

        return HasPermissionInternalAsync(userId, permissionCode, null, null, ct);
    }

    public Task<bool> HasPermissionAsync(
        long userId, 
        string permissionCode, 
        RoleAssignmentScopeType requiredScope,
        long? organizationId = null, 
        Guid? companyPublicId = null, 
        CancellationToken ct = default)
    {
        return HasPermissionInternalAsync(userId, permissionCode, requiredScope, (organizationId, companyPublicId), ct);
    }

    public Task<IReadOnlyCollection<string>> GetEffectivePermissionsAsync(long userId, CancellationToken ct = default)
    {
        return GetEffectivePermissionsInternalAsync(userId, null, null, ct);
    }

    public Task<IReadOnlyCollection<string>> GetEffectivePermissionsAsync(
        long userId,
        RoleAssignmentScopeType scope,
        long? organizationId = null,
        Guid? companyPublicId = null,
        CancellationToken ct = default)
    {
        return GetEffectivePermissionsInternalAsync(userId, scope, (organizationId, companyPublicId), ct);
    }

    private IReadOnlyList<string> GetRoles()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal is null)
        {
            return Array.Empty<string>();
        }

        return principal.Claims
            .Where(claim => claim.Type == IdentityClaimTypes.Role)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task<bool> HasPermissionInternalAsync(
        long userId,
        string permissionCode,
        RoleAssignmentScopeType? scope,
        (long? organizationId, Guid? companyPublicId)? context,
        CancellationToken ct)
    {
        var permissions = await GetEffectivePermissionsInternalAsync(userId, scope, context, ct);
        return permissions.Contains(permissionCode, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<IReadOnlyCollection<string>> GetEffectivePermissionsInternalAsync(
        long userId,
        RoleAssignmentScopeType? scope,
        (long? organizationId, Guid? companyPublicId)? context,
        CancellationToken ct)
    {
        var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var roles = GetRoles();

        foreach (var role in roles)
        {
            if (RolePermissions.TryGetValue(role, out var rolePermissions))
            {
                permissions.UnionWith(rolePermissions);
            }
        }

        if (roles.Count == 0)
        {
            var dbPermissions = await GetPermissionsFromDatabaseAsync(userId, scope, context, ct);
            permissions.UnionWith(dbPermissions);
        }

        return permissions.ToList();
    }

    private async Task<IReadOnlyCollection<string>> GetPermissionsFromDatabaseAsync(
        long userId,
        RoleAssignmentScopeType? scope,
        (long? organizationId, Guid? companyPublicId)? context,
        CancellationToken ct)
    {
        if (userId <= 0)
        {
            return Array.Empty<string>();
        }

        var organizationId = context?.organizationId;
        var companyPublicId = context?.companyPublicId;

        var assignments = _dbContext.RoleAssignments
            .AsNoTracking()
            .Where(assignment => assignment.UserId == userId && assignment.Status == RoleAssignmentStatus.Active)
            .Include(assignment => assignment.Role)
            .ThenInclude(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .AsQueryable();

        if (scope.HasValue)
        {
            assignments = scope.Value switch
            {
                RoleAssignmentScopeType.Platform => assignments.Where(a => a.ScopeType == RoleAssignmentScopeType.Platform),
                RoleAssignmentScopeType.Organization => assignments.Where(a =>
                    a.ScopeType == RoleAssignmentScopeType.Organization
                    && organizationId.HasValue
                    && a.OrganizationId == organizationId.Value),
                RoleAssignmentScopeType.Company => assignments.Where(a =>
                    a.ScopeType == RoleAssignmentScopeType.Company
                    && companyPublicId.HasValue
                    && a.CompanyPublicId == companyPublicId.Value),
                _ => assignments
            };
        }
        else
        {
            var currentUser = _httpContextAccessor.HttpContext?.RequestServices.GetService<ICurrentUserProvider>()?.GetCurrentUser();
            if (currentUser?.CompanyPublicId is { } currentCompanyPublicId)
            {
                assignments = assignments.Where(a =>
                    a.ScopeType == RoleAssignmentScopeType.Company
                    && a.CompanyPublicId == currentCompanyPublicId);
            }
            else
            {
                return Array.Empty<string>();
            }
        }

        var permissions = await assignments
            .SelectMany(a => a.Role.RolePermissions.Select(rp => rp.Permission.Code))
            .Distinct()
            .ToListAsync(ct);

        return permissions;
    }
}
