using ERP.Modules.Identity.Domain;
using ERP.Persistence;
using ERP.Shared.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ERP.Api.Services;

public sealed class RbacSeedService
{
    private readonly ErpDbContext _dbContext;
    private readonly ILogger<RbacSeedService> _logger;

    public RbacSeedService(ErpDbContext dbContext, ILogger<RbacSeedService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync(long? companyId, CancellationToken cancellationToken = default)
    {
        _ = companyId;
        var roleDefinitions = GetRoleDefinitions(roleName => CoreRbacRoleNames.Contains(roleName));
        var permissionDefinitions = GetPermissionDefinitions(IsCoreRbacPermissionCode);
        var rolePermissionMap = GetRolePermissionMap(
            IsCoreRbacPermissionCode,
            roleName => CoreRbacRoleNames.Contains(roleName),
            includeRolesWithoutPermissions: true);

        await UpsertRolesAsync(roleDefinitions, cancellationToken);
        await UpsertPermissionsAsync(permissionDefinitions, cancellationToken);
        await UpsertRolePermissionsAsync(rolePermissionMap, cancellationToken);
    }

    public async Task SeedOthersAsync(long? companyId, CancellationToken cancellationToken = default)
    {
        _ = companyId;
        var roleDefinitions = GetRoleDefinitions(roleName => !CoreRbacRoleNames.Contains(roleName));
        var permissionDefinitions = GetPermissionDefinitions(code => !IsCoreRbacPermissionCode(code));
        var rolePermissionMap = GetRolePermissionMap(
            code => !IsCoreRbacPermissionCode(code),
            roleName => !CoreRbacRoleNames.Contains(roleName));

        await UpsertRolesAsync(roleDefinitions, cancellationToken);
        await UpsertPermissionsAsync(permissionDefinitions, cancellationToken);
        await UpsertRolePermissionsAsync(rolePermissionMap, cancellationToken);
    }

    private async Task UpsertRolesAsync(
        IReadOnlyList<Role> roleDefinitions,
        CancellationToken cancellationToken)
    {
        var existingRoles = await _dbContext.Roles
            .Where(role => roleDefinitions.Select(definition => definition.Name).Contains(role.Name))
            .ToListAsync(cancellationToken);

        foreach (var definition in roleDefinitions)
        {
            var matchingRoles = existingRoles
                .Where(role => string.Equals(role.Name, definition.Name, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matchingRoles.Count == 0)
            {
                _dbContext.Roles.Add(definition);
                continue;
            }

foreach (var role in matchingRoles)
            {
                role.Update(definition.Name, definition.RequiredFeatureKey, true);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UpsertPermissionsAsync(
        IReadOnlyList<(string Code, string Name, string? Description, RoleAssignmentScopeType ScopeType)> definitions,
        CancellationToken cancellationToken)
    {
        var existingPermissions = await _dbContext.Permissions
            .Where(permission => definitions.Select(definition => definition.Code).Contains(permission.Code))
            .ToListAsync(cancellationToken);

        foreach (var (code, name, description, scopeType) in definitions)
        {
            var existing = existingPermissions.FirstOrDefault(permission =>
                string.Equals(permission.Code, code, StringComparison.OrdinalIgnoreCase));

            if (existing is null)
            {
                _dbContext.Permissions.Add(Permission.Create(code, name, description, scopeType));
                continue;
            }

            existing.Update(code, name, description, scopeType);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UpsertRolePermissionsAsync(
        IReadOnlyDictionary<string, IReadOnlyList<string>> rolePermissionMap,
        CancellationToken cancellationToken)
    {
        var roleNames = rolePermissionMap.Keys.ToArray();
        var permissionCodes = rolePermissionMap
            .SelectMany(pair => pair.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var roles = await _dbContext.Roles
            .Where(role => roleNames.Contains(role.Name))
            .ToListAsync(cancellationToken);

        var permissions = await _dbContext.Permissions
            .Where(permission => permissionCodes.Contains(permission.Code))
            .ToListAsync(cancellationToken);

        var existingRolePermissions = await _dbContext.RolePermissions
            .Where(rolePermission => roleNames.Contains(rolePermission.Role.Name))
            .ToListAsync(cancellationToken);

        var existingSet = existingRolePermissions
            .Select(rolePermission => (rolePermission.RoleId, rolePermission.PermissionId))
            .ToHashSet();

        foreach (var (roleName, permissionList) in rolePermissionMap)
        {
            var role = roles.FirstOrDefault(r => string.Equals(r.Name, roleName, StringComparison.OrdinalIgnoreCase));
            if (role is null)
            {
                _logger.LogWarning("RBAC seed skipped role permissions because role {RoleName} was not found.", roleName);
                continue;
            }

            foreach (var permissionCode in permissionList)
            {
                var permission = permissions.FirstOrDefault(p =>
                    string.Equals(p.Code, permissionCode, StringComparison.OrdinalIgnoreCase));
                if (permission is null)
                {
                    _logger.LogWarning("RBAC seed skipped permission {PermissionCode} for role {RoleName} because it was not found.", permissionCode, roleName);
                    continue;
                }

                if (existingSet.Contains((role.Id, permission.Id)))
                {
                    continue;
                }

                _dbContext.RolePermissions.Add(RolePermission.Create(role.Id, permission.Id));
                existingSet.Add((role.Id, permission.Id));
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IReadOnlyList<Role> GetRoleDefinitions(Func<string, bool> roleFilter)
    {
        var roleNames = typeof(RoleNames)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(string))
            .Select(field => (string)field.GetRawConstantValue()!)
            .Where(roleFilter)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(role => role)
            .ToArray();

        var metadata = GetRoleMetadata();

return roleNames
            .Select(roleName =>
            {
                if (!metadata.TryGetValue(roleName, out var info))
                {
                    info = new RoleMetadata(null, true, RoleAssignmentScopeType.Platform);
                }

                return Role.Create(
                    roleName,
                    requiredFeatureKey: info.RequiredFeatureKey,
                    isSystem: info.IsSystem,
                    scopeType: info.ScopeType);
            })
            .ToList();
    }

private static IReadOnlyDictionary<string, RoleMetadata> GetRoleMetadata()
        => new Dictionary<string, RoleMetadata>(StringComparer.OrdinalIgnoreCase)
        {
            // Platform scope roles
            [RoleNames.PlatformSuperAdmin] = new(null, true, RoleAssignmentScopeType.Platform),
            [RoleNames.PlatformSupport] = new(null, true, RoleAssignmentScopeType.Platform),
            [RoleNames.PlatformBilling] = new(null, true, RoleAssignmentScopeType.Platform),
            [RoleNames.PlatformAuditor] = new(null, true, RoleAssignmentScopeType.Platform),
            [RoleNames.FeaturePackAdder] = new(null, true, RoleAssignmentScopeType.Platform),
            [RoleNames.SystemEventProcessor] = new(null, true, RoleAssignmentScopeType.Platform),
            
            // Organization scope roles
            [RoleNames.OrganizationOwner] = new(null, true, RoleAssignmentScopeType.Organization),
            
            // Company scope roles
            [RoleNames.BillingClerk] = new(null, true, RoleAssignmentScopeType.Company),
            [RoleNames.CommercialManager] = new(null, true, RoleAssignmentScopeType.Company),
            [RoleNames.CompanyAccountant] = new(null, true, RoleAssignmentScopeType.Company),
            [RoleNames.CompanyAdmin] = new(null, true, RoleAssignmentScopeType.Company),
            [RoleNames.CompanyAuditor] = new(null, true, RoleAssignmentScopeType.Company),
            [RoleNames.CompanyOwner] = new(null, true, RoleAssignmentScopeType.Company),
            [RoleNames.FinanceClerk] = new(null, true, RoleAssignmentScopeType.Company),
            [RoleNames.FinanceManager] = new(null, true, RoleAssignmentScopeType.Company),
            [RoleNames.OperationsManager] = new(null, true, RoleAssignmentScopeType.Company),
            [RoleNames.Pharmacist] = new(FeatureKeys.PharmaceuticalBase, true, RoleAssignmentScopeType.Company),
            [RoleNames.PharmaceuticalChemist] = new(FeatureKeys.PharmaceuticalBase, true, RoleAssignmentScopeType.Company),
            [RoleNames.PharmaceuticalManager] = new(FeatureKeys.PharmaceuticalBase, true, RoleAssignmentScopeType.Company),
            [RoleNames.PharmacySupervisor] = new(FeatureKeys.PharmaceuticalBase, true, RoleAssignmentScopeType.Company),
            [RoleNames.PharmacyAuditor] = new(FeatureKeys.PharmaceuticalBase, true, RoleAssignmentScopeType.Company),
            [RoleNames.PurchasingClerk] = new(null, true, RoleAssignmentScopeType.Company),
            [RoleNames.RfidManager] = new(FeatureKeys.Rfid, true, RoleAssignmentScopeType.Company),
            [RoleNames.SalesClerk] = new(null, true, RoleAssignmentScopeType.Company),
            [RoleNames.WarehouseClerk] = new(null, true, RoleAssignmentScopeType.Company),
            [RoleNames.WarehouseManager] = new(null, true, RoleAssignmentScopeType.Company)
        };

    private readonly record struct RoleMetadata(string? RequiredFeatureKey, bool IsSystem, RoleAssignmentScopeType ScopeType);

    private static IReadOnlyList<(string Code, string Name, string? Description, RoleAssignmentScopeType ScopeType)> GetPermissionDefinitions(Func<string, bool> filter)
    {
        var codes = typeof(PermissionKeys)
            .GetNestedTypes(BindingFlags.Public | BindingFlags.Static)
            .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(field => field.FieldType == typeof(string) && field.IsLiteral && !field.IsInitOnly)
            .Select(field => (string)field.GetRawConstantValue()!)
            .Where(code => !string.IsNullOrWhiteSpace(code) && filter(code))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return codes
            .Select(code => (code, BuildPermissionName(code), (string?)null, DeterminePermissionScope(code)))
            .ToList();
    }

    private static RoleAssignmentScopeType DeterminePermissionScope(string code)
    {
        if (code.StartsWith("Platform.", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("Admin.", StringComparison.OrdinalIgnoreCase))
        {
            return RoleAssignmentScopeType.Platform;
        }

        if (code.StartsWith("Workspace.", StringComparison.OrdinalIgnoreCase))
        {
            return RoleAssignmentScopeType.Organization;
        }

        return RoleAssignmentScopeType.Company;
    }

    private static string BuildPermissionName(string code)
    {
        var segments = code.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 0)
        {
            return code;
        }

        var words = new List<string>(segments.Length * 2);
        foreach (var segment in segments)
        {
            var expanded = Regex.Replace(segment, "([a-z])([A-Z])", "$1 $2");
            expanded = Regex.Replace(expanded, "([A-Za-z])([0-9])", "$1 $2");
            expanded = Regex.Replace(expanded, "([0-9])([A-Za-z])", "$1 $2");
            words.Add(expanded);
        }

        return string.Join(" ", words);
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> GetRolePermissionMap(
        Func<string, bool> permissionFilter,
        Func<string, bool> roleFilter,
        bool includeRolesWithoutPermissions = false)
        => GetRolePermissionMap()
            .Where(pair => roleFilter(pair.Key))
            .Select(pair => new
            {
                pair.Key,
                Permissions = pair.Value
                    .Where(permissionFilter)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray()
            })
            .Where(entry => includeRolesWithoutPermissions || entry.Permissions.Length > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => (IReadOnlyList<string>)entry.Permissions,
                StringComparer.OrdinalIgnoreCase);

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> GetRolePermissionMap()
        => new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [RoleNames.PlatformSuperAdmin] = new[]
            {
                // Platform-scope permissions only
                PermissionKeys.Platform.FeaturesManage,
                PermissionKeys.Platform.GlobalMenuView,
                PermissionKeys.Platform.GlobalIAMManager,
                PermissionKeys.Platform.GlobalGEOManager,
                PermissionKeys.Platform.WsuEventsConsumption,
                PermissionKeys.Admin.UsersRead,
                PermissionKeys.Admin.UsersWrite,
                PermissionKeys.Admin.RolesAssign
            },
            [RoleNames.FeaturePackAdder] = new[]
            {
                PermissionKeys.Platform.FeaturesManage
            },
            [RoleNames.CompanyAdmin] = new[]
            {
                PermissionKeys.Company.AdminManage,
                PermissionKeys.Company.WarehouseManage,
                PermissionKeys.Company.StockManage
            },
            [RoleNames.CompanyAccountant] = new[]
            {
                PermissionKeys.Accounting.ChartOfAccountsRead,
                PermissionKeys.Accounting.ChartOfAccountsWrite,
                PermissionKeys.Accounting.JournalRead,
                PermissionKeys.Accounting.JournalAdjustmentsWrite,
                PermissionKeys.Accounting.PeriodClose,
                PermissionKeys.Accounting.TaxComplianceRead,
                PermissionKeys.Accounting.TaxComplianceWrite,
                PermissionKeys.Accounting.AutomationRulesManage
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
            [RoleNames.OperationsManager] = new[]
            {
                PermissionKeys.Inventory.StockRead,
                PermissionKeys.Inventory.MovementsCreate,
                PermissionKeys.Inventory.AdjustmentsCreate
            },
            [RoleNames.CompanyAuditor] = new[]
            {
                PermissionKeys.Accounting.ChartOfAccountsRead,
                PermissionKeys.Accounting.JournalRead,
                PermissionKeys.Accounting.TaxComplianceRead,
                PermissionKeys.Inventory.StockRead
            },
            [RoleNames.Pharmacist] = new[]
            {
                PermissionKeys.Pharmaceutical.DispenseCreate,
                PermissionKeys.Pharmaceutical.ControlledBookEntriesCreate,
                PermissionKeys.Pharmaceutical.ControlledBookExitsCreate
            },
            [RoleNames.PharmaceuticalChemist] = new[]
            {
                PermissionKeys.Pharmaceutical.DispenseCreate,
                PermissionKeys.Pharmaceutical.ControlledBookEntriesCreate,
                PermissionKeys.Pharmaceutical.ControlledBookExitsCreate,
                PermissionKeys.Inventory.AdjustmentsCreate
            },
            [RoleNames.PharmaceuticalManager] = GetPharmaceuticalPermissionCodes(),
            [RoleNames.PharmacySupervisor] = new[]
            {
                PermissionKeys.Pharmaceutical.DispenseCreate,
                PermissionKeys.Pharmaceutical.ControlledBookRead,
                PermissionKeys.Pharmaceutical.ControlledBookEntriesCreate,
                PermissionKeys.Pharmaceutical.ControlledBookExitsCreate,
                PermissionKeys.Pharmaceutical.ControlledBookAdjustmentsCreate,
                PermissionKeys.Pharmaceutical.ControlledBookExport
            },
            [RoleNames.PharmacyAuditor] = new[]
            {
                PermissionKeys.Pharmaceutical.ControlledBookRead,
                PermissionKeys.Pharmaceutical.ControlledBookExport
            },
            [RoleNames.CommercialManager] = new[]
            {
                PermissionKeys.Inventory.StockRead,
                PermissionKeys.Pricing.MarginWrite,
                PermissionKeys.Sales.QuotesRead,
                PermissionKeys.Sales.QuotesWrite,
                PermissionKeys.Sales.QuotesApprove,
                PermissionKeys.Documents.Read
            },
            // Platform roles (sin permisos temporalmente)
            [RoleNames.PlatformSupport] = Array.Empty<string>(),
            [RoleNames.PlatformBilling] = Array.Empty<string>(),
            [RoleNames.PlatformAuditor] = Array.Empty<string>(),
            [RoleNames.SystemEventProcessor] = new[]
            {
                PermissionKeys.Platform.WsuEventsConsumption,
                PermissionKeys.Platform.WsuEventsIngest
            },
            [RoleNames.RfidManager] = Array.Empty<string>(),
            // Organization scope roles
            [RoleNames.OrganizationOwner] = new[]
            {
                PermissionKeys.Workspace.CompaniesCreate,
                PermissionKeys.Workspace.MembersInvite,
                PermissionKeys.Workspace.Manage
            },
            // CompanyOwner has all company-scope permissions
            [RoleNames.CompanyOwner] = new[]
            {
                PermissionKeys.Company.AdminManage,
                PermissionKeys.Company.WarehouseManage,
                PermissionKeys.Company.StockManage
            },
            [RoleNames.FinanceManager] = Array.Empty<string>(),
            [RoleNames.SalesClerk] = new[]
            {
                PermissionKeys.Documents.Read,
                PermissionKeys.Documents.Write,
                PermissionKeys.Sales.QuotesRead,
                PermissionKeys.Sales.QuotesWrite
            },
            [RoleNames.PurchasingClerk] = Array.Empty<string>(),
            [RoleNames.WarehouseManager] = Array.Empty<string>(),
            [RoleNames.BillingClerk] = Array.Empty<string>()
        };

    private static readonly HashSet<string> CoreRbacPermissionCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        PermissionKeys.Platform.FeaturesManage,
        PermissionKeys.Platform.GlobalMenuView,
        PermissionKeys.Platform.GlobalIAMManager,
        PermissionKeys.Platform.GlobalGEOManager,
        PermissionKeys.Platform.WsuEventsConsumption,
        PermissionKeys.Platform.WsuEventsIngest,
        PermissionKeys.Admin.UsersRead,
        PermissionKeys.Admin.UsersWrite,
        PermissionKeys.Admin.RolesAssign,
        PermissionKeys.Workspace.CompaniesCreate,
        PermissionKeys.Workspace.MembersInvite,
        PermissionKeys.Workspace.Manage,
        PermissionKeys.Company.AdminManage,
        PermissionKeys.Company.WarehouseManage,
        PermissionKeys.Company.StockManage
    };

    private static readonly HashSet<string> CoreRbacRoleNames = new(StringComparer.OrdinalIgnoreCase)
    {
        RoleNames.CompanyAdmin,
        RoleNames.CompanyOwner,
        RoleNames.FeaturePackAdder,
        RoleNames.PlatformSuperAdmin,
        RoleNames.RfidManager,
        RoleNames.SystemEventProcessor
    };

    private static bool IsCoreRbacPermissionCode(string code)
        => CoreRbacPermissionCodes.Contains(code);

    private static IReadOnlyList<string> GetPharmaceuticalPermissionCodes()
        => typeof(PermissionKeys.Pharmaceutical)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(string) && field.IsLiteral && !field.IsInitOnly)
            .Select(field => (string)field.GetRawConstantValue()!)
            .Where(code => !string.IsNullOrWhiteSpace(code) && code.StartsWith("Pharmaceutical.", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
            .ToArray();
}
