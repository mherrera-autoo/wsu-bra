using System;
using System.Linq;
using System.Threading.Tasks;
using ERP.Modules.Identity.Contracts;
using ERP.Modules.Identity.Application.Services;
using ERP.Modules.Identity.Domain;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using ERP.Persistence;
using ERP.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ERP.Api.Integration.Tests;

public sealed class CompanyAccessResolverTests
{
    [Fact]
    public async Task ResolveAsync_MultiCompanyPortfolio_ReturnsAvailableWithoutAccess()
    {
        var options = CreateOptions();
        await using var dbContext = new ErpDbContext(options);

        var user = User.Create("accountant@studio.local", "hash", "salt");
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        var workspaceOrganization = Organization.Create(OrganizationType.MultiCompany, "Studio");
        await dbContext.Organizations.AddAsync(workspaceOrganization);
        await dbContext.SaveChangesAsync();

        var clientOrganization = Organization.Create(OrganizationType.Individual, "Client");
        await dbContext.Organizations.AddAsync(clientOrganization);
        await dbContext.SaveChangesAsync();

        var company = Company.Create(clientOrganization.Id, 0, "Client");
        await dbContext.Companies.AddAsync(company);
        await dbContext.SaveChangesAsync();

        var taxEntity = TaxEntity.Create(company.PublicId, "12345678-9", "Client");
        await dbContext.TaxEntities.AddAsync(taxEntity);
        await dbContext.SaveChangesAsync();

        company.UpdateTaxEntityId(taxEntity.Id);
        await dbContext.SaveChangesAsync();

        await dbContext.CompanyLinks.AddAsync(
            CompanyLink.Create(workspaceOrganization.Id, company.PublicId, CompanyLinkAccessType.ExternalAccountant));
        await dbContext.SaveChangesAsync();

        var resolver = CreateResolver(dbContext);

        var available = await resolver.ResolveAsync(user.Id, workspaceOrganization.Id, CompanyListScope.Available);

        Assert.Single(available);
        var entry = available.Single();
        Assert.Equal(company.PublicId, entry.CompanyPublicId);
        Assert.False(entry.HasAccess);
        Assert.True(entry.SourceFlags.HasFlag(CompanyAccessSource.PortfolioLink));
        Assert.Equal(CompanyLinkAccessType.ExternalAccountant, entry.AccessType);

        var operableOnly = await resolver.ResolveAsync(user.Id, workspaceOrganization.Id, CompanyListScope.OperableOnly);
        Assert.Empty(operableOnly);
    }

    [Fact]
    public async Task ResolveAsync_HoldingOrganizationCompany_ReturnsAvailableWithoutAccess()
    {
        var options = CreateOptions();
        await using var dbContext = new ErpDbContext(options);

        var user = User.Create("member@holding.local", "hash", "salt");
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        var holdingOrganization = Organization.Create(OrganizationType.Holding, "Holding");
        await dbContext.Organizations.AddAsync(holdingOrganization);
        await dbContext.SaveChangesAsync();

        var company = Company.Create(holdingOrganization.Id, 0, "Tenant");
        await dbContext.Companies.AddAsync(company);
        await dbContext.SaveChangesAsync();

        var taxEntity = TaxEntity.Create(company.PublicId, "99887766-5", "Tenant");
        await dbContext.TaxEntities.AddAsync(taxEntity);
        await dbContext.SaveChangesAsync();

        company.UpdateTaxEntityId(taxEntity.Id);
        await dbContext.SaveChangesAsync();

        var resolver = CreateResolver(dbContext);

        var available = await resolver.ResolveAsync(user.Id, holdingOrganization.Id, CompanyListScope.Available);

        Assert.Single(available);
        var entry = available.Single();
        Assert.False(entry.HasAccess);
        Assert.True(entry.SourceFlags.HasFlag(CompanyAccessSource.HoldingWorkspace));

        var operableOnly = await resolver.ResolveAsync(user.Id, holdingOrganization.Id, CompanyListScope.OperableOnly);
        Assert.Empty(operableOnly);
    }

    [Fact]
    public async Task ResolveAsync_DirectMembership_ReturnsOperableCompany()
    {
        var options = CreateOptions();
        await using var dbContext = new ErpDbContext(options);

        var user = User.Create("owner@tenant.local", "hash", "salt");
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        var organization = Organization.Create(OrganizationType.Individual, "Tenant");
        await dbContext.Organizations.AddAsync(organization);
        await dbContext.SaveChangesAsync();

        var company = Company.Create(organization.Id, 0, "Tenant");
        await dbContext.Companies.AddAsync(company);
        await dbContext.SaveChangesAsync();

        var taxEntity = TaxEntity.Create(company.PublicId, "10987654-3", "Tenant");
        await dbContext.TaxEntities.AddAsync(taxEntity);
        await dbContext.SaveChangesAsync();

        company.UpdateTaxEntityId(taxEntity.Id);
        await dbContext.SaveChangesAsync();

        await dbContext.CompanyUsers.AddAsync(CompanyUser.Create(company.PublicId, user.Id, CompanyUserStatus.Active));
        await dbContext.SaveChangesAsync();

        var resolver = CreateResolver(dbContext);

        var operableOnly = await resolver.ResolveAsync(user.Id, organization.Id, CompanyListScope.OperableOnly);

        Assert.Single(operableOnly);
        var entry = operableOnly.Single();
        Assert.True(entry.HasAccess);
        Assert.True(entry.SourceFlags.HasFlag(CompanyAccessSource.DirectMembership));
    }

    [Fact]
    public async Task ResolveAsync_DedupesAndAggregatesSources()
    {
        var options = CreateOptions();
        await using var dbContext = new ErpDbContext(options);

        var user = User.Create("accountant@studio.local", "hash", "salt");
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        var workspaceOrganization = Organization.Create(OrganizationType.MultiCompany, "Studio");
        await dbContext.Organizations.AddAsync(workspaceOrganization);
        await dbContext.SaveChangesAsync();

        var clientOrganization = Organization.Create(OrganizationType.Individual, "Client");
        await dbContext.Organizations.AddAsync(clientOrganization);
        await dbContext.SaveChangesAsync();

        var company = Company.Create(clientOrganization.Id, 0, "Client");
        await dbContext.Companies.AddAsync(company);
        await dbContext.SaveChangesAsync();

        var taxEntity = TaxEntity.Create(company.PublicId, "55667788-9", "Client");
        await dbContext.TaxEntities.AddAsync(taxEntity);
        await dbContext.SaveChangesAsync();

        company.UpdateTaxEntityId(taxEntity.Id);
        await dbContext.SaveChangesAsync();

        await dbContext.CompanyUsers.AddAsync(CompanyUser.Create(company.PublicId, user.Id, CompanyUserStatus.Active));
        await dbContext.CompanyLinks.AddAsync(
            CompanyLink.Create(workspaceOrganization.Id, company.PublicId, CompanyLinkAccessType.ExternalAccountant));
        await dbContext.SaveChangesAsync();

        var resolver = CreateResolver(dbContext);

        var available = await resolver.ResolveAsync(user.Id, workspaceOrganization.Id, CompanyListScope.Available);

        Assert.Single(available);
        var entry = available.Single();
        Assert.True(entry.HasAccess);
        Assert.True(entry.SourceFlags.HasFlag(CompanyAccessSource.DirectMembership));
        Assert.True(entry.SourceFlags.HasFlag(CompanyAccessSource.PortfolioLink));
    }

    private static DbContextOptions<ErpDbContext> CreateOptions()
        => new DbContextOptionsBuilder<ErpDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static CompanyAccessResolver CreateResolver(ErpDbContext dbContext)
        => new(
            dbContext,
            new ICompanyAccessSource[]
            {
                new DirectCompanyUsersSource(dbContext),
                new HoldingOrganizationCompaniesSource(dbContext),
                new MultiCompanyPortfolioSource(dbContext)
            });
}
