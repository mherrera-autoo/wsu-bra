using System.Net;
using System.Net.Http.Json;
using ERP.Api.Contracts.Onboarding;
using ERP.Modules.Identity.Application.Security;
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

public sealed class RoleAssignmentsScopeIntegrationTests
{
    [Fact]
    public async Task PlatformAssignment_ShouldNotGrantAccessToCompanyScopeEndpoint()
    {
        // Arrange
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long organizationId;
        long companyId;
        Guid companyPublicId;
        
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var organization = Organization.Create(OrganizationType.Individual, "Test Org");
            await dbContext.Organizations.AddAsync(organization);
            await dbContext.SaveChangesAsync();

            var company = Company.Create(organization.Id, 0, "Test Co");
            await dbContext.Companies.AddAsync(company);
            await dbContext.SaveChangesAsync();

            var taxEntity = TaxEntity.Create(company.PublicId, "TEST-001", "Test Co");
            await dbContext.TaxEntities.AddAsync(taxEntity);
            await dbContext.SaveChangesAsync();

            company.UpdateTaxEntityId(taxEntity.Id);
            await dbContext.SaveChangesAsync();

            organizationId = organization.Id;
            companyId = company.Id;
            companyPublicId = company.PublicId;
        }

        // Act - Create client with Platform SuperAdmin role at platform scope
        using var client = CreateClient(
            factory, 
            userId: 1, 
            organizationId: organizationId, 
            scope: "platform",
            companyId: null,
            companyPublicId: null,
            roles: "PlatformSuperAdmin");

        var response = await client.GetAsync($"/api/inventory/stock?productId=1&warehouseId=1");

        // Assert - Platform scope should not access company-scoped endpoint
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CompanyAssignmentWithPermission_ShouldGrantAccessToCompanyScopeEndpoint()
    {
        // Arrange
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long organizationId;
        long companyId;
        Guid companyPublicId;
        
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var organization = Organization.Create(OrganizationType.Individual, "Test Org");
            await dbContext.Organizations.AddAsync(organization);
            await dbContext.SaveChangesAsync();

            var company = Company.Create(organization.Id, 0, "Test Co");
            await dbContext.Companies.AddAsync(company);
            await dbContext.SaveChangesAsync();

            var taxEntity = TaxEntity.Create(company.PublicId, "TEST-001", "Test Co");
            await dbContext.TaxEntities.AddAsync(taxEntity);
            await dbContext.SaveChangesAsync();

            company.UpdateTaxEntityId(taxEntity.Id);
            await dbContext.SaveChangesAsync();

            organizationId = organization.Id;
            companyId = company.Id;
            companyPublicId = company.PublicId;
        }

        // Act - Create client with CompanyAdmin role at company scope
        using var client = CreateClient(
            factory, 
            userId: 1, 
            organizationId: organizationId, 
            scope: "tenant",
            companyId: companyId,
            companyPublicId: companyPublicId,
            roles: "CompanyAdmin");

        var response = await client.GetAsync($"/api/inventory/stock?productId=1&warehouseId=1");

        // Assert - Company scope with correct permission should access endpoint
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CompanyScopeEndpoint_WithoutCompanyPublicId_ShouldReturnCompanyContextRequired()
    {
        // Arrange
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long organizationId;
        long companyId;
        
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var organization = Organization.Create(OrganizationType.Individual, "Test Org");
            await dbContext.Organizations.AddAsync(organization);
            await dbContext.SaveChangesAsync();

            var company = Company.Create(organization.Id, 0, "Test Co");
            await dbContext.Companies.AddAsync(company);
            await dbContext.SaveChangesAsync();

            var taxEntity = TaxEntity.Create(company.PublicId, "TEST-001", "Test Co");
            await dbContext.TaxEntities.AddAsync(taxEntity);
            await dbContext.SaveChangesAsync();

            company.UpdateTaxEntityId(taxEntity.Id);
            await dbContext.SaveChangesAsync();

            organizationId = organization.Id;
            companyId = company.Id;
        }

        // Act - Create client missing companyPublicId header
        using var client = CreateClient(
            factory, 
            userId: 1, 
            organizationId: organizationId, 
            scope: "tenant",
            companyId: companyId,
            companyPublicId: null, // Missing company context
            roles: "CompanyAdmin");

        var response = await client.GetAsync($"/api/inventory/stock?productId=1&warehouseId=1");

        // Assert - Should return unauthorized due to missing company context
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PlatformSuperAdmin_WithoutCompanyRoleAssignment_ShouldNotAccessCompanyEndpoints()
    {
        // Arrange
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long organizationId;
        long companyId;
        Guid companyPublicId;
        
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var organization = Organization.Create(OrganizationType.Individual, "Test Org");
            await dbContext.Organizations.AddAsync(organization);
            await dbContext.SaveChangesAsync();

            var company = Company.Create(organization.Id, 0, "Test Co");
            await dbContext.Companies.AddAsync(company);
            await dbContext.SaveChangesAsync();

            var taxEntity = TaxEntity.Create(company.PublicId, "TEST-001", "Test Co");
            await dbContext.TaxEntities.AddAsync(taxEntity);
            await dbContext.SaveChangesAsync();

            company.UpdateTaxEntityId(taxEntity.Id);
            await dbContext.SaveChangesAsync();

            organizationId = organization.Id;
            companyId = company.Id;
            companyPublicId = company.PublicId;
        }

        // Act - Create client with Platform SuperAdmin role at platform scope ONLY
        // No Company role assignment should be created
        using var client = CreateClient(
            factory, 
            userId: 1, 
            organizationId: organizationId, 
            scope: "platform",
            companyId: null,
            companyPublicId: null,
            roles: "PlatformSuperAdmin");

        var response = await client.GetAsync($"/api/inventory/stock?productId=1&warehouseId=1");

        // Assert - Platform SuperAdmin without Company role assignment should not access company endpoints
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task OrganizationMember_WithoutOrganizationRoleAssignment_ShouldNotCreateCompany()
    {
        // Arrange
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long organizationId;
        
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var organization = Organization.Create(OrganizationType.MultiCompany, "Test Org");
            await dbContext.Organizations.AddAsync(organization);
            await dbContext.SaveChangesAsync();

            organizationId = organization.Id;
        }

        // Act - Create client with OrganizationMember but NO OrganizationOwner RoleAssignment
        using var client = CreateClient(
            factory, 
            userId: 1, 
            organizationId: organizationId, 
            scope: "platform",
            companyId: null,
            companyPublicId: null,
            roles: "CompanyAdmin"); // Company role but no Organization role

        var onboardingRequest = new OnboardIndividualRequest("ORG-TEST-001", "Test Company");
        var response = await client.PostAsJsonAsync("/api/onboarding/individual", onboardingRequest);

        // Assert - Should fail because user doesn't have Organization permission
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task OrganizationOwner_WithRoleAssignment_ShouldCreateCompany()
    {
        // Arrange
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long organizationId;
        long userId = 1;
        
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var organization = Organization.Create(OrganizationType.MultiCompany, "Test Org");
            await dbContext.Organizations.AddAsync(organization);
            await dbContext.SaveChangesAsync();

            // Create user
            var user = User.Create($"user-{userId}@test.com", "test-hash", "test-salt");
            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();

            // Create OrganizationMember for membership tracking
            var organizationMember = OrganizationMember.Create(organization.Id, userId, OrganizationRole.Owner);
            await dbContext.OrganizationMembers.AddAsync(organizationMember);
            
            // Create OrganizationOwner RoleAssignment for actual authorization
            var orgOwnerRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == RoleNames.OrganizationOwner);
            if (orgOwnerRole == null)
            {
                orgOwnerRole = Role.Create(RoleNames.OrganizationOwner, isSystem: true, scopeType: RoleAssignmentScopeType.Organization);
                await dbContext.Roles.AddAsync(orgOwnerRole);
                await dbContext.SaveChangesAsync();
            }
            
            var orgRoleAssignment = RoleAssignment.CreateOrganization(userId, orgOwnerRole.Id, organization.Id);
            await dbContext.RoleAssignments.AddAsync(orgRoleAssignment);
            await dbContext.SaveChangesAsync();

            organizationId = organization.Id;
        }

        // Act - Create client with OrganizationOwner RoleAssignment
        using var client = CreateClient(
            factory, 
            userId: userId, 
            organizationId: organizationId, 
            scope: "platform",
            companyId: null,
            companyPublicId: null,
            roles: "OrganizationOwner");

        var onboardingRequest = new OnboardIndividualRequest("ORG-TEST-001", "Test Company");
        var response = await client.PostAsJsonAsync("/api/onboarding/individual", onboardingRequest);

        // Assert - Should succeed because user has Organization permission
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var onboardingResult = await response.Content.ReadFromJsonAsync<OnboardingResult>();
        Assert.NotNull(onboardingResult);
        Assert.True(onboardingResult.OrganizationId > 0);
        Assert.NotNull(onboardingResult.CompanyPublicId);
        Assert.True(onboardingResult.CompanyId > 0);
    }

    [Fact]
    public async Task Onboarding_ShouldCreateRoleAssignmentsAndCompanyUsers()
    {
        // Arrange
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long userId = 1;
        long organizationId = 1;
        
        // Create a user in the database first
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var user = User.Create($"user-{userId}@test.com", "test-hash", "test-salt");
            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();
        }
        
        // Act - Execute onboarding through the API with proper authentication
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.OrganizationIdHeader, organizationId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.ScopeHeader, "platform");
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, RoleNames.OrganizationOwner);
        
        var onboardingRequest = new OnboardIndividualRequest("ONBOARD-TEST-001", "Onboarding Test Co");
        
        var response = await client.PostAsJsonAsync("/api/onboarding/individual", onboardingRequest);
        
        // Debug - Log response if not OK
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Onboarding failed with status {response.StatusCode}: {errorContent}");
        }
        
        // Assert - Onboarding should succeed
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var onboardingResult = await response.Content.ReadFromJsonAsync<OnboardingResult>();
        Assert.NotNull(onboardingResult);
        Assert.True(onboardingResult.OrganizationId > 0);
        Assert.NotNull(onboardingResult.CompanyPublicId);
        Assert.True(onboardingResult.CompanyId > 0);
        Assert.NotNull(onboardingResult.TenantToken);

        // Verify database state
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            
            // Check Organization exists
            var organization = await dbContext.Organizations.FindAsync(onboardingResult.OrganizationId);
            Assert.NotNull(organization);
            Assert.Equal(OrganizationType.Individual, organization.AccountType);
            
            // Check Company exists
            var company = await dbContext.Companies.FindAsync(onboardingResult.CompanyId);
            Assert.NotNull(company);
            Assert.Equal(onboardingResult.CompanyPublicId, company.PublicId);
            
            // Check RoleAssignments were created
            var roleAssignments = await dbContext.RoleAssignments
                .Where(ra => ra.UserId == userId)
                .ToListAsync();
            
            // Should have 3 role assignments:
            // 1. OrganizationOwner at Organization scope
            // 2. CompanyOwner at Company scope
            // 3. CompanyAdmin at Company scope
            Assert.Equal(3, roleAssignments.Count);
            
            var orgRoleAssignment = roleAssignments.FirstOrDefault(ra => ra.ScopeType == RoleAssignmentScopeType.Organization);
            Assert.NotNull(orgRoleAssignment);
            
            var companyRoleAssignments = roleAssignments.Where(ra => ra.ScopeType == RoleAssignmentScopeType.Company).ToList();
            Assert.Equal(2, companyRoleAssignments.Count);
            Assert.All(companyRoleAssignments, ra => Assert.Equal(onboardingResult.CompanyPublicId, ra.CompanyPublicId));
            Assert.All(companyRoleAssignments, ra => Assert.Null(ra.OrganizationId));
            
            // Check CompanyUser was created (principal operational component)
            var companyUser = await dbContext.CompanyUsers
                .FirstOrDefaultAsync(cu => cu.UserId == userId && cu.CompanyPublicId == onboardingResult.CompanyPublicId);
            Assert.NotNull(companyUser);
            Assert.Equal(CompanyUserStatus.Active, companyUser.Status);
        }
    }

    private static HttpClient CreateClient(
        ApiWebApplicationFactory factory,
        long userId,
        long organizationId,
        string scope,
        long? companyId,
        Guid? companyPublicId,
        string roles)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.OrganizationIdHeader, organizationId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.ScopeHeader, scope);
        
        if (companyId.HasValue)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyIdHeader, companyId.Value.ToString());
        }
        
        if (companyPublicId.HasValue)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyPublicIdHeader, companyPublicId.Value.ToString());
        }

        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, roles);
        return client;
    }
}
