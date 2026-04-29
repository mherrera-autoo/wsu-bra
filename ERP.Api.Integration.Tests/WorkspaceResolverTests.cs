using ERP.Api.Controllers;
using ERP.Modules.Identity.Contracts;
using ERP.Modules.Identity.Application.Services;
using ERP.Modules.Identity.Domain;
using ERP.Modules.MasterData.Domain;
using ERP.Persistence;
using ERP.Persistence.Persistence;
using ERP.Persistence.Services;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using ERP.Modules.MasterData.Contracts;

namespace ERP.Api.Integration.Tests;

public sealed class WorkspaceResolverTests
{
    [Fact]
    public async Task ResolveWorkspaceOrganizationIdAsync_PrefersMultiCompany()
    {
        var options = CreateOptions();
        await using var dbContext = new ErpDbContext(options);

        var user = User.Create("studio@local", "hash", "salt");
        await dbContext.Users.AddAsync(user);

        var multiCompany = Organization.Create(OrganizationType.MultiCompany, "Studio");
        var individual = Organization.Create(OrganizationType.Individual, "Client");
        await dbContext.Organizations.AddRangeAsync(multiCompany, individual);

        await dbContext.OrganizationMembers.AddAsync(OrganizationMember.Create(multiCompany.Id, user.Id, OrganizationRole.Owner));
        await dbContext.OrganizationMembers.AddAsync(OrganizationMember.Create(individual.Id, user.Id, OrganizationRole.Owner));
        await dbContext.SaveChangesAsync();

        var resolver = new WorkspaceResolver(dbContext, NullLogger<WorkspaceResolver>.Instance);

        var workspaceOrganizationId = await resolver.ResolveWorkspaceOrganizationIdAsync(user.Id);

        Assert.Equal(multiCompany.Id, workspaceOrganizationId);
    }

    [Fact]
    public async Task ResolveWorkspaceOrganizationIdAsync_PrefersHoldingWhenNoMultiCompany()
    {
        var options = CreateOptions();
        await using var dbContext = new ErpDbContext(options);

        var user = User.Create("holding@local", "hash", "salt");
        await dbContext.Users.AddAsync(user);

        var holding = Organization.Create(OrganizationType.Holding, "Holding");
        var individual = Organization.Create(OrganizationType.Individual, "Client");
        await dbContext.Organizations.AddRangeAsync(holding, individual);

        await dbContext.OrganizationMembers.AddAsync(OrganizationMember.Create(holding.Id, user.Id, OrganizationRole.Owner));
        await dbContext.OrganizationMembers.AddAsync(OrganizationMember.Create(individual.Id, user.Id, OrganizationRole.Owner));
        await dbContext.SaveChangesAsync();

        var resolver = new WorkspaceResolver(dbContext, NullLogger<WorkspaceResolver>.Instance);

        var workspaceOrganizationId = await resolver.ResolveWorkspaceOrganizationIdAsync(user.Id);

        Assert.Equal(holding.Id, workspaceOrganizationId);
    }

    [Fact]
    public async Task ResolveWorkspaceOrganizationIdAsync_UsesIndividualWhenOnlyOption()
    {
        var options = CreateOptions();
        await using var dbContext = new ErpDbContext(options);

        var user = User.Create("individual@local", "hash", "salt");
        await dbContext.Users.AddAsync(user);

        var individual = Organization.Create(OrganizationType.Individual, "Client");
        await dbContext.Organizations.AddAsync(individual);
        await dbContext.OrganizationMembers.AddAsync(OrganizationMember.Create(individual.Id, user.Id, OrganizationRole.Owner));
        await dbContext.SaveChangesAsync();

        var resolver = new WorkspaceResolver(dbContext, NullLogger<WorkspaceResolver>.Instance);

        var workspaceOrganizationId = await resolver.ResolveWorkspaceOrganizationIdAsync(user.Id);

        Assert.Equal(individual.Id, workspaceOrganizationId);
    }

    [Fact]
    public async Task ListCompanies_UsesWorkspaceResolverForPortfolioLinks()
    {
        var options = CreateOptions();
        await using var dbContext = new ErpDbContext(options);

        var user = User.Create("accountant@studio.local", "hash", "salt");
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        var studioOrganization = Organization.Create(OrganizationType.MultiCompany, "Studio");
        var clientOrganization = Organization.Create(OrganizationType.Individual, "Client");
        await dbContext.Organizations.AddRangeAsync(studioOrganization, clientOrganization);
        await dbContext.SaveChangesAsync();

        await dbContext.OrganizationMembers.AddAsync(OrganizationMember.Create(studioOrganization.Id, user.Id, OrganizationRole.Owner));
        await dbContext.OrganizationMembers.AddAsync(OrganizationMember.Create(clientOrganization.Id, user.Id, OrganizationRole.Owner));
        await dbContext.SaveChangesAsync();

        var company = Company.Create(clientOrganization.Id, 0, "Client Company");
        await dbContext.Companies.AddAsync(company);
        await dbContext.SaveChangesAsync();

        var taxEntity = TaxEntity.Create(company.PublicId, "12345678-9", "Client");
        await dbContext.TaxEntities.AddAsync(taxEntity);
        await dbContext.SaveChangesAsync();

        company.UpdateTaxEntityId(taxEntity.Id);
        await dbContext.SaveChangesAsync();

        await dbContext.CompanyLinks.AddAsync(
            CompanyLink.Create(studioOrganization.Id, company.PublicId, CompanyLinkAccessType.ExternalAccountant));
        await dbContext.CompanyUsers.AddAsync(CompanyUser.Create(company.PublicId, user.Id, CompanyUserStatus.Active));
        await dbContext.SaveChangesAsync();

        var sources = new ICompanyAccessSource[]
        {
            new DirectCompanyUsersSource(dbContext),
            new HoldingOrganizationCompaniesSource(dbContext),
            new MultiCompanyPortfolioSource(dbContext)
        };
        var accessResolver = new CompanyAccessResolver(dbContext, sources);
        var accessService = new CompanyAccessService(accessResolver);
        var workspaceResolver = new WorkspaceResolver(dbContext, NullLogger<WorkspaceResolver>.Instance);

        var currentUser = new CurrentUser(
            user.Id,
            clientOrganization.Id,
            "platform",
            null,
            0,
            "accountant@studio.local",
            Array.Empty<string>()
            );
        var currentUserProvider = new TestCurrentUserProvider(currentUser);

        var controller = new IdentityController(
            null!,
            null!,
            new ConfigurationBuilder().Build(),
            currentUserProvider,
            null!,
            accessService,
            null!,
            null!,
            workspaceResolver,
            null!,
            null!);

        var result = await controller.ListCompanies(CompanyListScope.Available, null, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var companies = Assert.IsAssignableFrom<IReadOnlyList<CompanyAccessSummary>>(okResult.Value);
        var summary = Assert.Single(companies);

        Assert.True(summary.SourceFlags.HasFlag(CompanyAccessSource.PortfolioLink));
        Assert.Equal(CompanyLinkAccessType.ExternalAccountant, summary.AccessType);
        Assert.Equal(company.PublicId, summary.CompanyPublicId);
    }

    private static DbContextOptions<ErpDbContext> CreateOptions()
        => new DbContextOptionsBuilder<ErpDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private sealed class TestCurrentUserProvider : ICurrentUserProvider
    {
        private readonly CurrentUser _currentUser;

        public TestCurrentUserProvider(CurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        public long? UserId => _currentUser.UserId;
        public IReadOnlyCollection<string> Roles => _currentUser.Roles;

        public string? Source => "tests";

        public CurrentUser? GetCurrentUser() => _currentUser;
    }
}
