using System.Net;
using System.Net.Http.Json;
using ERP.Api.Contracts.Identity;
using ERP.Modules.Identity.Contracts;
using ERP.Modules.Identity.Domain;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using ERP.Persistence;
using ERP.Shared.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ERP.Api.Integration.Tests;

public sealed class IdentityRolePermissionStatusIntegrationTests
{
    [Fact]
    public async Task PatchRoleStatus_Disable_ReturnsNoContent_AndUpdatesRole()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long companyId;
        Guid companyPublicId;
        long organizationId;
        long roleId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (companyId, companyPublicId, organizationId) = await SeedCompanyAsync(dbContext, "Role Status Co");
            await SeedPlatformIamPermissionAsync(dbContext, userId: 1);

            var role = Role.Create("Status Role", "Test", scopeType: RoleAssignmentScopeType.Platform);
            await dbContext.Roles.AddAsync(role);
            await dbContext.SaveChangesAsync();

            roleId = role.Id;
        }

        using var client = CreateClient(factory, companyId, companyPublicId, organizationId);
        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/identity/roles/{roleId}/status")
        {
            Content = JsonContent.Create(new UpdateRoleStatusRequest { IsActive = false })
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var role = await dbContext.Roles.FirstAsync(r => r.Id == roleId);
            Assert.False(role.IsActive);
        }
    }

    [Fact]
    public async Task PatchRoleStatus_Enable_ReturnsNoContent_AndUpdatesRole()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long companyId;
        Guid companyPublicId;
        long organizationId;
        long roleId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (companyId, companyPublicId, organizationId) = await SeedCompanyAsync(dbContext, "Role Enable Co");
            await SeedPlatformIamPermissionAsync(dbContext, userId: 1);

            var role = Role.Create("Disabled Role", "Test", scopeType: RoleAssignmentScopeType.Platform);
            role.Disable();
            await dbContext.Roles.AddAsync(role);
            await dbContext.SaveChangesAsync();

            roleId = role.Id;
        }

        using var client = CreateClient(factory, companyId, companyPublicId, organizationId);
        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/identity/roles/{roleId}/status")
        {
            Content = JsonContent.Create(new UpdateRoleStatusRequest { IsActive = true })
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var role = await dbContext.Roles.FirstAsync(r => r.Id == roleId);
            Assert.True(role.IsActive);
        }
    }

    [Fact]
    public async Task PatchRoleStatus_WhenMissing_ReturnsNotFound()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long companyId;
        Guid companyPublicId;
        long organizationId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (companyId, companyPublicId, organizationId) = await SeedCompanyAsync(dbContext, "Role Missing Co");
            await SeedPlatformIamPermissionAsync(dbContext, userId: 1);
        }

        using var client = CreateClient(factory, companyId, companyPublicId, organizationId);
        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, "/api/identity/roles/9999/status")
        {
            Content = JsonContent.Create(new UpdateRoleStatusRequest { IsActive = false })
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PatchPlatformRoleStatus_Disable_ReturnsNoContent_AndUpdatesRole()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long companyId;
        Guid companyPublicId;
        long organizationId;
        long roleId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (companyId, companyPublicId, organizationId) = await SeedCompanyAsync(dbContext, "Platform Role Status Co");
            await SeedPlatformIamPermissionAsync(dbContext, userId: 1);

            var role = Role.Create("Platform Status Role", "Test", scopeType: RoleAssignmentScopeType.Platform);
            await dbContext.Roles.AddAsync(role);
            await dbContext.SaveChangesAsync();

            roleId = role.Id;
        }

        using var client = CreateClient(factory, companyId, companyPublicId, organizationId);
        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/platform/roles/{roleId}/status")
        {
            Content = JsonContent.Create(new UpdateRoleStatusRequest { IsActive = false })
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var role = await dbContext.Roles.FirstAsync(r => r.Id == roleId);
            Assert.False(role.IsActive);
        }
    }

    [Fact]
    public async Task PatchPlatformRoleStatus_Enable_ReturnsNoContent_AndUpdatesRole()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long companyId;
        Guid companyPublicId;
        long organizationId;
        long roleId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (companyId, companyPublicId, organizationId) = await SeedCompanyAsync(dbContext, "Platform Role Enable Co");
            await SeedPlatformIamPermissionAsync(dbContext, userId: 1);

            var role = Role.Create("Platform Disabled Role", "Test", scopeType: RoleAssignmentScopeType.Platform);
            role.Disable();
            await dbContext.Roles.AddAsync(role);
            await dbContext.SaveChangesAsync();

            roleId = role.Id;
        }

        using var client = CreateClient(factory, companyId, companyPublicId, organizationId);
        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/platform/roles/{roleId}/status")
        {
            Content = JsonContent.Create(new UpdateRoleStatusRequest { IsActive = true })
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var role = await dbContext.Roles.FirstAsync(r => r.Id == roleId);
            Assert.True(role.IsActive);
        }
    }

    [Fact]
    public async Task PatchPlatformRoleStatus_WhenMissing_ReturnsNotFound()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long companyId;
        Guid companyPublicId;
        long organizationId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (companyId, companyPublicId, organizationId) = await SeedCompanyAsync(dbContext, "Platform Role Missing Co");
            await SeedPlatformIamPermissionAsync(dbContext, userId: 1);
        }

        using var client = CreateClient(factory, companyId, companyPublicId, organizationId);
        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, "/api/platform/roles/9999/status")
        {
            Content = JsonContent.Create(new UpdateRoleStatusRequest { IsActive = false })
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetIdentityRoles_ReturnsAllRoles_WithAssignmentFlagForCurrentUser()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long companyId;
        Guid companyPublicId;
        long organizationId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (companyId, companyPublicId, organizationId) = await SeedCompanyAsync(dbContext, "Identity Roles By User Co");
            await SeedPlatformIamPermissionAsync(dbContext, userId: 1);

            var userOneRole = Role.Create("Assigned To User One", "Test", scopeType: RoleAssignmentScopeType.Platform);
            var userTwoRole = Role.Create("Assigned To User Two", "Test", scopeType: RoleAssignmentScopeType.Platform);
            var companyRole = Role.Create("Company Only Role", "Test", scopeType: RoleAssignmentScopeType.Company);
            await dbContext.Roles.AddRangeAsync(userOneRole, userTwoRole, companyRole);
            await dbContext.SaveChangesAsync();

            await dbContext.RoleAssignments.AddAsync(RoleAssignment.CreatePlatform(1, userOneRole.Id));
            await dbContext.RoleAssignments.AddAsync(RoleAssignment.CreatePlatform(2, userTwoRole.Id));
            await dbContext.SaveChangesAsync();
        }

        using var client = CreateClient(factory, companyId, companyPublicId, organizationId, userId: 1);
        var response = await client.GetAsync("/api/identity/roles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var roles = await response.Content.ReadFromJsonAsync<List<RoleSummary>>();
        Assert.NotNull(roles);
        Assert.Contains(roles!, role => role.Name == "Assigned To User One" && role.IsAssigned);
        Assert.Contains(roles!, role => role.Name == "Assigned To User Two" && !role.IsAssigned);
        Assert.DoesNotContain(roles!, role => role.Name == "Company Only Role");
    }

    [Fact]
    public async Task GetPlatformRoles_WithUserPublicId_ReturnsRoles_WithIsAssignedToUserFlag()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long companyId;
        Guid companyPublicId;
        long organizationId;
        Guid targetUserPublicId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (companyId, companyPublicId, organizationId) = await SeedCompanyAsync(dbContext, "Platform Roles By User Co");
            await SeedPlatformIamPermissionAsync(dbContext, userId: 1);

            var assignedRole = Role.Create("Platform Assigned To User", "Test", scopeType: RoleAssignmentScopeType.Platform);
            var unassignedRole = Role.Create("Platform Not Assigned To User", "Test", scopeType: RoleAssignmentScopeType.Platform);
            await dbContext.Roles.AddRangeAsync(assignedRole, unassignedRole);
            await dbContext.SaveChangesAsync();

            await dbContext.RoleAssignments.AddAsync(RoleAssignment.CreatePlatform(2, assignedRole.Id));
            await dbContext.SaveChangesAsync();

            targetUserPublicId = await dbContext.Users
                .Where(user => user.Id == 2)
                .Select(user => user.PublicId)
                .SingleAsync();
        }

        using var client = CreateClient(factory, companyId, companyPublicId, organizationId, userId: 1);
        var response = await client.GetAsync($"/api/platform/roles?userPublicId={targetUserPublicId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var roles = await response.Content.ReadFromJsonAsync<List<PlatformRoleSummary>>();
        Assert.NotNull(roles);
        Assert.Contains(roles!, role => role.Name == "Platform Assigned To User" && role.IsAssignedToUser);
        Assert.Contains(roles!, role => role.Name == "Platform Not Assigned To User" && !role.IsAssignedToUser);
    }

    [Fact]
    public async Task PutPlatformRoles_UpsertUserRoles_AddsAndRemovesAssignments()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long companyId;
        Guid companyPublicId;
        long organizationId;
        Guid targetUserPublicId;
        Guid roleToAddPublicId;
        Guid roleToRemovePublicId;
        Guid companyRoleToAddPublicId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (companyId, companyPublicId, organizationId) = await SeedCompanyAsync(dbContext, "Platform Roles Upsert Co");
            await SeedPlatformIamPermissionAsync(dbContext, userId: 1);

            var roleToAdd = Role.Create("Platform Role Add", "Test", scopeType: RoleAssignmentScopeType.Platform);
            var roleToRemove = Role.Create("Platform Role Remove", "Test", scopeType: RoleAssignmentScopeType.Platform);
            var companyRoleToAdd = Role.Create("Company Role Add", "Test", scopeType: RoleAssignmentScopeType.Company);
            await dbContext.Roles.AddRangeAsync(roleToAdd, roleToRemove, companyRoleToAdd);
            await dbContext.SaveChangesAsync();

            await dbContext.RoleAssignments.AddAsync(RoleAssignment.CreatePlatform(2, roleToRemove.Id));
            await dbContext.SaveChangesAsync();

            targetUserPublicId = await dbContext.Users
                .Where(user => user.Id == 2)
                .Select(user => user.PublicId)
                .SingleAsync();

            roleToAddPublicId = roleToAdd.PublicId;
            roleToRemovePublicId = roleToRemove.PublicId;
            companyRoleToAddPublicId = companyRoleToAdd.PublicId;
        }

        using var client = CreateClient(factory, companyId, companyPublicId, organizationId, userId: 1);
        var response = await client.PutAsJsonAsync("/api/platform/roles", new UpsertPlatformUserRolesRequest(
            targetUserPublicId,
            new[] { roleToAddPublicId, companyRoleToAddPublicId },
            new[] { roleToRemovePublicId }));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var targetUserId = await dbContext.Users
                .Where(user => user.PublicId == targetUserPublicId)
                .Select(user => user.Id)
                .SingleAsync();

            var roleToAddId = await dbContext.Roles
                .Where(role => role.PublicId == roleToAddPublicId)
                .Select(role => role.Id)
                .SingleAsync();

            var roleToRemoveId = await dbContext.Roles
                .Where(role => role.PublicId == roleToRemovePublicId)
                .Select(role => role.Id)
                .SingleAsync();

            var companyRoleToAddId = await dbContext.Roles
                .Where(role => role.PublicId == companyRoleToAddPublicId)
                .Select(role => role.Id)
                .SingleAsync();

            Assert.Contains(await dbContext.RoleAssignments.ToListAsync(), assignment =>
                assignment.UserId == targetUserId
                && assignment.RoleId == roleToAddId
                && assignment.ScopeType == RoleAssignmentScopeType.Platform
                && assignment.Status == RoleAssignmentStatus.Active);

            Assert.Contains(await dbContext.RoleAssignments.ToListAsync(), assignment =>
                assignment.UserId == targetUserId
                && assignment.RoleId == roleToRemoveId
                && assignment.ScopeType == RoleAssignmentScopeType.Platform
                && assignment.Status == RoleAssignmentStatus.Inactive);

            Assert.Contains(await dbContext.RoleAssignments.ToListAsync(), assignment =>
                assignment.UserId == targetUserId
                && assignment.RoleId == companyRoleToAddId
                && assignment.ScopeType == RoleAssignmentScopeType.Company
                && assignment.CompanyPublicId == companyPublicId
                && assignment.Status == RoleAssignmentStatus.Active);
        }
    }

    [Fact]
    public async Task PutPlatformRoles_UpsertUserRoles_WithMissingRole_ReturnsBadRequest()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long companyId;
        Guid companyPublicId;
        long organizationId;
        Guid targetUserPublicId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (companyId, companyPublicId, organizationId) = await SeedCompanyAsync(dbContext, "Platform Roles Missing Role Co");
            await SeedPlatformIamPermissionAsync(dbContext, userId: 1);

            targetUserPublicId = await dbContext.Users
                .Where(user => user.Id == 2)
                .Select(user => user.PublicId)
                .SingleAsync();
        }

        using var client = CreateClient(factory, companyId, companyPublicId, organizationId, userId: 1);
        var response = await client.PutAsJsonAsync("/api/platform/roles", new UpsertPlatformUserRolesRequest(
            targetUserPublicId,
            new[] { Guid.NewGuid() },
            Array.Empty<Guid>()));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PatchPermissionStatus_Disable_ReturnsNoContent_AndUpdatesPermission()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long companyId;
        Guid companyPublicId;
        long organizationId;
        long permissionId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (companyId, companyPublicId, organizationId) = await SeedCompanyAsync(dbContext, "Permission Status Co");
            await SeedPlatformIamPermissionAsync(dbContext, userId: 1);

            var permission = Permission.Create("Test.Permission.Disable", "Test Permission Disable");
            await dbContext.Permissions.AddAsync(permission);
            await dbContext.SaveChangesAsync();
            permissionId = permission.Id;
        }

        using var client = CreateClient(factory, companyId, companyPublicId, organizationId);
        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/identity/permissions/{permissionId}/status")
        {
            Content = JsonContent.Create(new UpdatePermissionStatusRequest { IsActive = false })
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var permission = await dbContext.Permissions.FirstAsync(p => p.Id == permissionId);
            Assert.False(permission.IsActive);
        }
    }

    [Fact]
    public async Task PatchPermissionStatus_Enable_ReturnsNoContent_AndUpdatesPermission()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long companyId;
        Guid companyPublicId;
        long organizationId;
        long permissionId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (companyId, companyPublicId, organizationId) = await SeedCompanyAsync(dbContext, "Permission Enable Co");
            await SeedPlatformIamPermissionAsync(dbContext, userId: 1);

            var permission = Permission.Create("Test.Permission.Enable", "Test Permission Enable");
            permission.Disable();
            await dbContext.Permissions.AddAsync(permission);
            await dbContext.SaveChangesAsync();
            permissionId = permission.Id;
        }

        using var client = CreateClient(factory, companyId, companyPublicId, organizationId);
        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/identity/permissions/{permissionId}/status")
        {
            Content = JsonContent.Create(new UpdatePermissionStatusRequest { IsActive = true })
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var permission = await dbContext.Permissions.FirstAsync(p => p.Id == permissionId);
            Assert.True(permission.IsActive);
        }
    }

    [Fact]
    public async Task PatchPermissionStatus_WhenMissing_ReturnsNotFound()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long companyId;
        Guid companyPublicId;
        long organizationId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (companyId, companyPublicId, organizationId) = await SeedCompanyAsync(dbContext, "Permission Missing Co");
            await SeedPlatformIamPermissionAsync(dbContext, userId: 1);
        }

        using var client = CreateClient(factory, companyId, companyPublicId, organizationId);
        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, "/api/identity/permissions/9999/status")
        {
            Content = JsonContent.Create(new UpdatePermissionStatusRequest { IsActive = false })
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PatchPlatformPermissionStatus_Disable_ReturnsNoContent_AndUpdatesPermission()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long companyId;
        Guid companyPublicId;
        long organizationId;
        long permissionId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (companyId, companyPublicId, organizationId) = await SeedCompanyAsync(dbContext, "Platform Permission Status Co");
            await SeedPlatformIamPermissionAsync(dbContext, userId: 1);

            var permission = Permission.Create("Test.Platform.Permission.Disable", "Test Platform Permission Disable");
            await dbContext.Permissions.AddAsync(permission);
            await dbContext.SaveChangesAsync();
            permissionId = permission.Id;
        }

        using var client = CreateClient(factory, companyId, companyPublicId, organizationId);
        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/platform/permissions/{permissionId}/status")
        {
            Content = JsonContent.Create(new UpdatePermissionStatusRequest { IsActive = false })
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var permission = await dbContext.Permissions.FirstAsync(p => p.Id == permissionId);
            Assert.False(permission.IsActive);
        }
    }

    [Fact]
    public async Task PatchPlatformPermissionStatus_Enable_ReturnsNoContent_AndUpdatesPermission()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long companyId;
        Guid companyPublicId;
        long organizationId;
        long permissionId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (companyId, companyPublicId, organizationId) = await SeedCompanyAsync(dbContext, "Platform Permission Enable Co");
            await SeedPlatformIamPermissionAsync(dbContext, userId: 1);

            var permission = Permission.Create("Test.Platform.Permission.Enable", "Test Platform Permission Enable");
            permission.Disable();
            await dbContext.Permissions.AddAsync(permission);
            await dbContext.SaveChangesAsync();
            permissionId = permission.Id;
        }

        using var client = CreateClient(factory, companyId, companyPublicId, organizationId);
        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/platform/permissions/{permissionId}/status")
        {
            Content = JsonContent.Create(new UpdatePermissionStatusRequest { IsActive = true })
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var permission = await dbContext.Permissions.FirstAsync(p => p.Id == permissionId);
            Assert.True(permission.IsActive);
        }
    }

    [Fact]
    public async Task PatchPlatformPermissionStatus_WhenMissing_ReturnsNotFound()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long companyId;
        Guid companyPublicId;
        long organizationId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (companyId, companyPublicId, organizationId) = await SeedCompanyAsync(dbContext, "Platform Permission Missing Co");
            await SeedPlatformIamPermissionAsync(dbContext, userId: 1);
        }

        using var client = CreateClient(factory, companyId, companyPublicId, organizationId);
        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, "/api/platform/permissions/9999/status")
        {
            Content = JsonContent.Create(new UpdatePermissionStatusRequest { IsActive = false })
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task<(long companyId, Guid companyPublicId, long organizationId)> SeedCompanyAsync(
        ErpDbContext dbContext,
        string name)
    {
        var organization = Organization.Create(OrganizationType.Individual, $"{name} Org");
        await dbContext.Organizations.AddAsync(organization);
        await dbContext.SaveChangesAsync();

        var company = Company.Create(organization.Id, 0, name);
        await dbContext.Companies.AddAsync(company);
        await dbContext.SaveChangesAsync();

        var taxEntity = TaxEntity.Create(company.PublicId, $"{name.ToUpperInvariant()}-IAM", name);
        await dbContext.TaxEntities.AddAsync(taxEntity);
        await dbContext.SaveChangesAsync();

        company.UpdateTaxEntityId(taxEntity.Id);
        await dbContext.SaveChangesAsync();

        return (company.Id, company.PublicId, organization.Id);
    }

    private static async Task SeedPlatformIamPermissionAsync(ErpDbContext dbContext, long userId)
    {
        var permission = Permission.Create(PermissionKeys.Platform.GlobalIAMManager, "Platform IAM Manage");
        await dbContext.Permissions.AddAsync(permission);

        var role = Role.Create("IAM Manager", scopeType: RoleAssignmentScopeType.Platform);
        await dbContext.Roles.AddAsync(role);
        await dbContext.SaveChangesAsync();

        await dbContext.RolePermissions.AddAsync(RolePermission.Create(role.Id, permission.Id));
        await dbContext.RoleAssignments.AddAsync(RoleAssignment.CreatePlatform(userId, role.Id));
        await dbContext.SaveChangesAsync();
    }

    private static HttpClient CreateClient(
        ApiWebApplicationFactory factory,
        long companyId,
        Guid companyPublicId,
        long organizationId,
        long userId = 1)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.OrganizationIdHeader, organizationId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.ScopeHeader, "tenant");
        client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyIdHeader, companyId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyPublicIdHeader, companyPublicId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, string.Empty);
        return client;
    }
}
