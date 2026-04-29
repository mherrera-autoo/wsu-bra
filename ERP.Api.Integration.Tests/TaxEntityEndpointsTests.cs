using ERP.Modules.Identity.Contracts;
using ERP.Modules.Identity.Domain;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using ERP.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ERP.Api.Integration.Tests;

public sealed class TaxEntityEndpointsTests
{
    [Fact]
    public async Task ListTaxEntities_ReturnsAccessibleTaxEntitiesWithMetadata()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long userId;
        long tenantCompanyId;
        Guid tenantCompanyPublicId;
        long portfolioTaxEntityId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var user = User.Create("accountant@workspace.local", "hash", "salt");
            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();
            userId = user.Id;

            var workspaceOrganization = Organization.Create(OrganizationType.MultiCompany, "Workspace");
            await dbContext.Organizations.AddAsync(workspaceOrganization);
            await dbContext.SaveChangesAsync();

            await dbContext.OrganizationMembers.AddAsync(
                OrganizationMember.Create(workspaceOrganization.Id, userId, OrganizationRole.Member));
            await dbContext.SaveChangesAsync();

            var tenantOrganization = Organization.Create(OrganizationType.Individual, "Tenant Org");
            await dbContext.Organizations.AddAsync(tenantOrganization);
            await dbContext.SaveChangesAsync();

            var tenantCompany = Company.Create(tenantOrganization.Id, 0, "Tenant");
            await dbContext.Companies.AddAsync(tenantCompany);
            await dbContext.SaveChangesAsync();

            var tenantTaxEntity = TaxEntity.Create(tenantCompany.PublicId, "11111111-1", "Tenant");
            await dbContext.TaxEntities.AddAsync(tenantTaxEntity);
            await dbContext.SaveChangesAsync();

            tenantCompany.UpdateTaxEntityId(tenantTaxEntity.Id);
            await dbContext.SaveChangesAsync();
            tenantCompanyId = tenantCompany.Id;
            tenantCompanyPublicId = tenantCompany.PublicId;

            await dbContext.CompanyUsers.AddAsync(
                CompanyUser.Create(tenantCompanyPublicId, userId, CompanyUserStatus.Active));
            await dbContext.SaveChangesAsync();

            var portfolioOrganization = Organization.Create(OrganizationType.Individual, "Portfolio Org");
            await dbContext.Organizations.AddAsync(portfolioOrganization);
            await dbContext.SaveChangesAsync();

            var portfolioCompany = Company.Create(portfolioOrganization.Id, 0, "Portfolio");
            await dbContext.Companies.AddAsync(portfolioCompany);
            await dbContext.SaveChangesAsync();

            var portfolioTaxEntity = TaxEntity.Create(portfolioCompany.PublicId, "22222222-2", "Portfolio");
            await dbContext.TaxEntities.AddAsync(portfolioTaxEntity);
            await dbContext.SaveChangesAsync();
            portfolioTaxEntityId = portfolioTaxEntity.Id;

            portfolioCompany.UpdateTaxEntityId(portfolioTaxEntity.Id);
            await dbContext.SaveChangesAsync();

            await dbContext.CompanyLinks.AddAsync(
                CompanyLink.Create(
                    workspaceOrganization.Id,
                    portfolioCompany.PublicId,
                    CompanyLinkAccessType.ExternalAccountant));
            await dbContext.SaveChangesAsync();
        }

        using var client = CreateClient(factory, userId, tenantCompanyId, tenantCompanyPublicId);
        var response = await client.GetAsync("/api/identity/tax-entities");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<IReadOnlyList<TaxEntityAccessSummary>>();
        Assert.NotNull(payload);
        Assert.Equal(2, payload!.Count);

        var portfolioEntry = payload.Single(entry => entry.TaxEntityId == portfolioTaxEntityId);
        Assert.False(portfolioEntry.HasOperableCompany);
        Assert.True(portfolioEntry.SourceFlags.HasFlag(CompanyAccessSource.PortfolioLink));
        Assert.Equal(1, portfolioEntry.CompanyCount);
    }

    [Fact]
    public async Task GetCurrentTaxEntity_ReturnsCompanyTaxEntity()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long userId;
        long tenantCompanyId;
        Guid tenantCompanyPublicId;
        long tenantTaxEntityId;
        long otherTaxEntityId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var user = User.Create("owner@tenant.local", "hash", "salt");
            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();
            userId = user.Id;

            var organization = Organization.Create(OrganizationType.Individual, "Tenant Org");
            await dbContext.Organizations.AddAsync(organization);
            await dbContext.SaveChangesAsync();

            var tenantCompany = Company.Create(organization.Id, 0, "Tenant");
            await dbContext.Companies.AddAsync(tenantCompany);
            await dbContext.SaveChangesAsync();

            var tenantTaxEntity = TaxEntity.Create(tenantCompany.PublicId, "33333333-3", "Tenant");
            await dbContext.TaxEntities.AddAsync(tenantTaxEntity);
            await dbContext.SaveChangesAsync();
            tenantTaxEntityId = tenantTaxEntity.Id;

            tenantCompany.UpdateTaxEntityId(tenantTaxEntity.Id);
            await dbContext.SaveChangesAsync();
            tenantCompanyId = tenantCompany.Id;
            tenantCompanyPublicId = tenantCompany.PublicId;

            await dbContext.CompanyUsers.AddAsync(
                CompanyUser.Create(tenantCompanyPublicId, userId, CompanyUserStatus.Active));
            await dbContext.SaveChangesAsync();

            var otherCompany = Company.Create(organization.Id, 0, "Other");
            await dbContext.Companies.AddAsync(otherCompany);
            await dbContext.SaveChangesAsync();

            var otherTaxEntity = TaxEntity.Create(otherCompany.PublicId, "44444444-4", "Other");
            await dbContext.TaxEntities.AddAsync(otherTaxEntity);
            await dbContext.SaveChangesAsync();
            otherTaxEntityId = otherTaxEntity.Id;

            otherCompany.UpdateTaxEntityId(otherTaxEntity.Id);
            await dbContext.SaveChangesAsync();
        }

        using var client = CreateClient(factory, userId, tenantCompanyId, tenantCompanyPublicId);
        var response = await client.GetAsync("/api/identity/tax-entities/current");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ERP.Api.Contracts.Identity.TaxEntitySummary>();
        Assert.NotNull(payload);
        Assert.Equal(tenantTaxEntityId, payload!.Id);
        Assert.NotEqual(otherTaxEntityId, payload.Id);
    }

    private static HttpClient CreateClient(
        ApiWebApplicationFactory factory,
        long userId,
        long companyId,
        Guid companyPublicId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.OrganizationIdHeader, "1");
        client.DefaultRequestHeaders.Add(TestAuthHandler.ScopeHeader, "tenant");
        client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyIdHeader, companyId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyPublicIdHeader, companyPublicId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, string.Empty);
        return client;
    }
}
