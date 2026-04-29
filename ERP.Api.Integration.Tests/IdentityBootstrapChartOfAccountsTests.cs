using System.Net;
using System.Net.Http.Json;
using ERP.Api.Contracts.Identity;
using ERP.Persistence;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace ERP.Api.Integration.Tests;

public sealed class IdentityBootstrapChartOfAccountsTests
{
    [Fact]
    public async Task Bootstrap_CreatesCompanyChartOfAccountsFromTemplate()
    {
        using var factory = new IdentityBootstrapWebApplicationFactory(Guid.NewGuid().ToString());
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/identity/bootstrap",
            new BootstrapRequest(factory.BootstrapToken));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
        var company = await dbContext.Companies.SingleAsync(entity => entity.Name == "Bootstrap Company");
        var accountCount = await dbContext.Accounts.CountAsync(account => account.CompanyId == company.Id);

        Assert.True(accountCount > 0);
    }

    [Fact]
    public async Task Bootstrap_PreservesTemplateHierarchy()
    {
        using var factory = new IdentityBootstrapWebApplicationFactory(Guid.NewGuid().ToString());
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/identity/bootstrap",
            new BootstrapRequest(factory.BootstrapToken));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
        var company = await dbContext.Companies.SingleAsync(entity => entity.Name == "Bootstrap Company");

        var asset = await dbContext.Accounts.SingleAsync(
            account => account.CompanyId == company.Id && account.Code == "1");
        var currentAsset = await dbContext.Accounts.SingleAsync(
            account => account.CompanyId == company.Id && account.Code == "1.1");
        var cashGroup = await dbContext.Accounts.SingleAsync(
            account => account.CompanyId == company.Id && account.Code == "1.1.1");
        var cash = await dbContext.Accounts.SingleAsync(
            account => account.CompanyId == company.Id && account.Code == "1.1.1.01");

        Assert.Equal(asset.Id, currentAsset.ParentId);
        Assert.Equal(currentAsset.Id, cashGroup.ParentId);
        Assert.Equal(cashGroup.Id, cash.ParentId);
    }

    [Fact]
    public async Task Bootstrap_IsIdempotentForCompanyChartOfAccounts()
    {
        using var factory = new IdentityBootstrapWebApplicationFactory(Guid.NewGuid().ToString());
        using var client = factory.CreateClient();

        var firstResponse = await client.PostAsJsonAsync(
            "/api/identity/bootstrap",
            new BootstrapRequest(factory.BootstrapToken));

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
        var company = await dbContext.Companies.SingleAsync(entity => entity.Name == "Bootstrap Company");
        var initialCount = await dbContext.Accounts.CountAsync(account => account.CompanyId == company.Id);

        var secondResponse = await client.PostAsJsonAsync(
            "/api/identity/bootstrap",
            new BootstrapRequest(factory.BootstrapToken));

        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        var finalCount = await dbContext.Accounts.CountAsync(account => account.CompanyId == company.Id);

        Assert.Equal(initialCount, finalCount);
    }

    [Fact]
    public async Task Bootstrap_IsIdempotentForCompanyOwnerRoleAssignment()
    {
        using var factory = new IdentityBootstrapWebApplicationFactory(Guid.NewGuid().ToString());
        using var client = factory.CreateClient();

        var firstResponse = await client.PostAsJsonAsync(
            "/api/identity/bootstrap",
            new BootstrapRequest(factory.BootstrapToken));
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var secondResponse = await client.PostAsJsonAsync(
            "/api/identity/bootstrap",
            new BootstrapRequest(factory.BootstrapToken));
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();

        var company = await dbContext.Companies.SingleAsync(entity => entity.Name == "Bootstrap Company");
        var user = await dbContext.Users.SingleAsync(entity => entity.Email == "admin@example.com");
        var companyOwnerRole = await dbContext.Roles.SingleAsync(entity => entity.Name == RoleNames.CompanyOwner);

        var assignments = await dbContext.RoleAssignments.CountAsync(entity =>
            entity.UserId == user.Id
            && entity.RoleId == companyOwnerRole.Id
            && entity.CompanyPublicId == company.PublicId);

        Assert.Equal(1, assignments);
    }

    private sealed class IdentityBootstrapWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName;

        public IdentityBootstrapWebApplicationFactory(string databaseName)
        {
            _databaseName = databaseName;
        }

        public string BootstrapToken { get; } = "bootstrap-token";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                var settings = new Dictionary<string, string?>
                {
                    ["Bootstrap:AdminEmail"] = "admin@example.com",
                    ["Bootstrap:AdminPassword"] = "P@ssword123!",
                    ["Bootstrap:BootstrapToken"] = BootstrapToken,
                    ["Bootstrap:Organization:AccountType"] = "Individual",
                    ["Bootstrap:Organization:Name"] = "Bootstrap Org",
                    ["Bootstrap:Organization:DisplayName"] = "Bootstrap Org",
                    ["Bootstrap:Company:0:Name"] = "Bootstrap Company",
                    ["Bootstrap:Company:0:TaxId"] = "BOOTSTRAP-1",
                    ["Bootstrap:Company:0:OrganizationName"] = "Bootstrap Org"
                };

                config.AddInMemoryCollection(settings);
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ErpDbContext>>();
                services.AddDbContext<ErpDbContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));
            });
        }
    }
}
