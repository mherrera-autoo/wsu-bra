using System.Net;
using System.Net.Http.Json;
using ERP.Api.Contracts.Accounting;
using ERP.Api.Contracts.Billing;
using ERP.Modules.Accounting.Application.Reports;
using ERP.Modules.Identity.Application.Services;
using ERP.Modules.Identity.Domain;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using ERP.Persistence;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace ERP.Api.Integration.Tests;

public sealed class AccountingFlowIntegrationTests
{
    [Fact]
    public async Task InvoicePosting_UpdatesJournalReceivablesAndReports()
    {
        const long customerId = 1200;
        long companyId;
        long organizationId;
        Guid companyPublicId;
        using var factory = new AccountingApiWebApplicationFactory(Guid.NewGuid().ToString());
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var organization = Organization.Create(OrganizationType.Individual, "Accounting Org");
            await dbContext.Organizations.AddAsync(organization);
            await dbContext.SaveChangesAsync();

            var company = Company.Create(organization.Id, 0, "Accounting Company");
            await dbContext.Companies.AddAsync(company);
            await dbContext.SaveChangesAsync();

            var taxEntity = TaxEntity.Create(company.PublicId, "ACC-TEST", "Accounting Org");
            await dbContext.TaxEntities.AddAsync(taxEntity);
            await dbContext.SaveChangesAsync();

            company.UpdateTaxEntityId(taxEntity.Id);
            await dbContext.SaveChangesAsync();

            var currency = Currency.Create("USD", 840, "US Dollar", "$", 2, 1, true);
            await dbContext.Currencies.AddAsync(currency);
            await dbContext.SaveChangesAsync();

            await dbContext.CompanyCurrencies.AddAsync(CompanyCurrency.Create(company.Id, currency.Id, isDefault: true, isActive: true));
            await dbContext.SaveChangesAsync();

            companyId = company.Id;
            companyPublicId = company.PublicId;
            organizationId = organization.Id;
        }

        using var client = CreateClient(factory, organizationId, companyId, companyPublicId);

        var bootstrapResponse = await client.PostAsJsonAsync(
            "/api/accounting/bootstrap",
            new BootstrapAccountingRequest("USD"));
        Assert.Equal(HttpStatusCode.OK, bootstrapResponse.StatusCode);

        var accounts = await client.GetFromJsonAsync<List<AccountSummary>>(
            "/api/accounting/accounts?includeSystem=true");
        Assert.NotNull(accounts);

        var receivableAccountId = accounts!
            .Single(account => account.Code == "1.1.2")
            .Id;
        var revenueAccountId = accounts!
            .Single(account => account.Code == "4.1")
            .Id;

        var issueRequest = new IssueInvoiceRequest(
            customerId,
            receivableAccountId,
            revenueAccountId,
            new List<IssueInvoiceLineRequest>
            {
                new(2000, 2m, 50m, "USD")
            });

        var issueResponse = await client.PostAsJsonAsync("/api/billing/invoices/issue", issueRequest);
        Assert.Equal(HttpStatusCode.OK, issueResponse.StatusCode);

        var issuedInvoice = await issueResponse.Content.ReadFromJsonAsync<IssueInvoiceResponse>();
        Assert.NotNull(issuedInvoice);

        var balanceSheet = await client.GetFromJsonAsync<BalanceSheetReport>(
            $"/api/reports/balance-sheet?companyId={companyId}");
        Assert.NotNull(balanceSheet);
        Assert.Equal(100m, balanceSheet!.Assets.Total);

        var incomeStatement = await client.GetFromJsonAsync<IncomeStatementReport>(
            $"/api/reports/income-statement?companyId={companyId}");
        Assert.NotNull(incomeStatement);
        Assert.Equal(100m, incomeStatement!.Revenues.Total);
        Assert.Equal(100m, incomeStatement.NetIncome);
    }

    private static HttpClient CreateClient(
        WebApplicationFactory<Program> factory,
        long organizationId,
        long companyId,
        Guid companyPublicId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, "1");
        client.DefaultRequestHeaders.Add(TestAuthHandler.OrganizationIdHeader, organizationId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.ScopeHeader, "tenant");
        client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyIdHeader, companyId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyPublicIdHeader, companyPublicId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, string.Empty);
        return client;
    }

    private sealed record IssueInvoiceResponse(long Id, long Number);

    private sealed class AccountingApiWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName;

        public AccountingApiWebApplicationFactory(string databaseName)
        {
            _databaseName = databaseName;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ErpDbContext>>();
                services.AddDbContext<ErpDbContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));

                services.RemoveAll<IRbacService>();
                services.AddSingleton<IRbacService, AllowAllRbacService>();

                services.RemoveAll<IUnitOfWork>();
                services.AddScoped<IUnitOfWork, TestUnitOfWork>();

                services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthHandler.Scheme;
                        options.DefaultChallengeScheme = TestAuthHandler.Scheme;
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.Scheme, _ => { });
            });
        }
    }

private sealed class AllowAllRbacService : IRbacService
    {
        public Task<bool> HasPermissionAsync(long userId, string permissionCode, CancellationToken ct = default)
            => Task.FromResult(true);

        public Task<bool> HasPermissionAsync(
            long userId, 
            string permissionCode, 
            RoleAssignmentScopeType requiredScope,
            long? organizationId = null, 
            Guid? companyPublicId = null, 
            CancellationToken ct = default)
            => Task.FromResult(true);

        public Task<IReadOnlyCollection<string>> GetEffectivePermissionsAsync(
            long userId,
            CancellationToken ct = default)
            => Task.FromResult<IReadOnlyCollection<string>>(Array.Empty<string>());

        public Task<IReadOnlyCollection<string>> GetEffectivePermissionsAsync(
            long userId,
            RoleAssignmentScopeType scope,
            long? organizationId = null,
            Guid? companyPublicId = null,
            CancellationToken ct = default)
            => Task.FromResult<IReadOnlyCollection<string>>(Array.Empty<string>());
    }
}
