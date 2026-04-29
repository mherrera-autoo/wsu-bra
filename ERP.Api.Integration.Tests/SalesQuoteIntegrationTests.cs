using ERP.Api.Authorization;
using ERP.Modules.Identity.Domain;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using ERP.Modules.Sales.Contracts;
using ERP.Modules.Sales.Domain;
using ERP.Persistence;
using ERP.Shared.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ContractSalesQuoteStatus = ERP.Modules.Sales.Contracts.SalesQuoteStatus;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace ERP.Api.Integration.Tests;

public sealed class SalesQuoteIntegrationTests
{
    [Fact]
    public async Task CreateQuote_CreatesDraft()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long userId;
        CompanySeed company;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (company, userId) = await SeedCompanyAsync(dbContext);
            await GrantSalesPermissionsAsync(dbContext, company.CompanyPublicId, userId, PermissionKeys.Sales.QuotesWrite);
        }

        using var client = CreateClient(factory, company, userId);

        var request = new CreateSalesQuoteApiRequest(
            "Customer A",
            "Initial quote",
            "USD",
            new List<CreateSalesQuoteLineRequest>
            {
                new(null, "Service A", 2m, 50m, 0m),
                new(null, "Service B", 1m, 25m, 0m)
            });

        var response = await client.PostAsJsonAsync("/api/sales/quotes", request);
        response.EnsureSuccessStatusCode();

        var detail = await response.Content.ReadFromJsonAsync<SalesQuoteDetail>();
        Assert.NotNull(detail);
        Assert.Equal(ContractSalesQuoteStatus.Draft, detail!.Status);
        Assert.Equal(125m, detail.Total);
    }

    [Fact]
    public async Task SubmitQuote_PublishesOutboxEvent()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long userId;
        CompanySeed company;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (company, userId) = await SeedCompanyAsync(dbContext);
            await GrantSalesPermissionsAsync(dbContext, company.CompanyPublicId, userId, PermissionKeys.Sales.QuotesWrite);
        }

        using var client = CreateClient(factory, company, userId);
        var quoteId = await CreateQuoteAsync(client);

        var submitResponse = await client.PostAsync($"/api/sales/quotes/{quoteId}/submit", null);
        submitResponse.EnsureSuccessStatusCode();

        using var scopeCheck = factory.Services.CreateScope();
        var dbContextCheck = scopeCheck.ServiceProvider.GetRequiredService<ErpDbContext>();
        var outboxTypes = await dbContextCheck.OutboxMessages
            .Where(message => message.Type == "sales.quote.submitted")
            .Select(message => message.Type)
            .ToListAsync();
        Assert.Single(outboxTypes);
    }

    [Fact]
    public async Task ApproveQuote_PublishesOutboxEvent()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long userId;
        CompanySeed company;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (company, userId) = await SeedCompanyAsync(dbContext);
            await GrantSalesPermissionsAsync(dbContext, company.CompanyPublicId, userId,
                PermissionKeys.Sales.QuotesWrite,
                PermissionKeys.Sales.QuotesApprove);
        }

        using var client = CreateClient(factory, company, userId);
        var quoteId = await CreateQuoteAsync(client);
        await client.PostAsync($"/api/sales/quotes/{quoteId}/submit", null);

        var approveResponse = await client.PostAsync($"/api/sales/quotes/{quoteId}/approve", null);
        approveResponse.EnsureSuccessStatusCode();

        using var scopeCheck = factory.Services.CreateScope();
        var dbContextCheck = scopeCheck.ServiceProvider.GetRequiredService<ErpDbContext>();
        var outboxTypes = await dbContextCheck.OutboxMessages
            .Where(message => message.Type == "sales.quote.approved")
            .Select(message => message.Type)
            .ToListAsync();
        Assert.Single(outboxTypes);
    }

    [Fact]
    public async Task RejectQuote_PublishesOutboxEventWithReason()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long userId;
        CompanySeed company;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (company, userId) = await SeedCompanyAsync(dbContext);
            await GrantSalesPermissionsAsync(dbContext, company.CompanyPublicId, userId,
                PermissionKeys.Sales.QuotesWrite,
                PermissionKeys.Sales.QuotesApprove);
        }

        using var client = CreateClient(factory, company, userId);
        var quoteId = await CreateQuoteAsync(client);
        await client.PostAsync($"/api/sales/quotes/{quoteId}/submit", null);

        var rejectResponse = await client.PostAsJsonAsync($"/api/sales/quotes/{quoteId}/reject", new RejectSalesQuoteRequest("No budget"));
        rejectResponse.EnsureSuccessStatusCode();

        using var scopeCheck = factory.Services.CreateScope();
        var dbContextCheck = scopeCheck.ServiceProvider.GetRequiredService<ErpDbContext>();
        var outbox = await dbContextCheck.OutboxMessages
            .Where(message => message.Type == "sales.quote.rejected")
            .Select(message => message.PayloadJson)
            .SingleAsync();

        using var document = JsonDocument.Parse(outbox);
        Assert.Equal("No budget", document.RootElement.GetProperty("reason").GetString());
    }

    [Fact]
    public async Task SalesQuotes_FilterByCompanyId()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long userId;
        CompanySeed companyA;
        CompanySeed companyB;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (companyA, companyB, userId) = await SeedTwoCompaniesAsync(dbContext);
            await GrantSalesPermissionsAsync(dbContext, companyA.CompanyPublicId, userId, PermissionKeys.Sales.QuotesRead);

            var quoteA = SalesQuote.Create(companyA.CompanyId, "1", "Customer A", null, "USD", userId);
            quoteA.AddLine(1, null, "Line A", 1m, 10m, 0m);
            var quoteB = SalesQuote.Create(companyB.CompanyId, "1", "Customer B", null, "USD", userId);
            quoteB.AddLine(1, null, "Line B", 1m, 20m, 0m);

            await dbContext.SalesQuotes.AddRangeAsync(quoteA, quoteB);
            await dbContext.SaveChangesAsync();
        }

        using var client = CreateClient(factory, companyA, userId);

        var list = await client.GetFromJsonAsync<PagedResult<SalesQuoteListItem>>("/api/sales/quotes");
        Assert.NotNull(list);
        Assert.Single(list!.Items);
        Assert.Equal("Customer A", list.Items[0].CustomerName);

        var otherQuote = await GetOtherCompanyQuotePublicId(factory, companyB.CompanyId);
        var response = await client.GetAsync($"/api/sales/quotes/{otherQuote}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task<Guid> CreateQuoteAsync(HttpClient client)
    {
        var request = new CreateSalesQuoteApiRequest(
            "Customer A",
            null,
            "USD",
            new List<CreateSalesQuoteLineRequest>
            {
                new(null, "Service A", 1m, 100m, 0m)
            });

        var response = await client.PostAsJsonAsync("/api/sales/quotes", request);
        response.EnsureSuccessStatusCode();
        var detail = await response.Content.ReadFromJsonAsync<SalesQuoteDetail>();
        return detail!.PublicId;
    }

    private static HttpClient CreateClient(ApiWebApplicationFactory factory, CompanySeed company, long userId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.OrganizationIdHeader, company.OrganizationId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.ScopeHeader, "tenant");
        client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyIdHeader, company.CompanyId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyPublicIdHeader, company.CompanyPublicId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, string.Empty);
        return client;
    }

    private static async Task<(CompanySeed Company, long UserId)> SeedCompanyAsync(ErpDbContext dbContext)
    {
        var organization = Organization.Create(OrganizationType.Individual, "Sales Org");
        await dbContext.Organizations.AddAsync(organization);
        await dbContext.SaveChangesAsync();

        var company = Company.Create(organization.Id, 0, "Sales Company");
        await dbContext.Companies.AddAsync(company);
        await dbContext.SaveChangesAsync();

        var taxEntity = TaxEntity.Create(company.PublicId, "SALESCOMP", "Sales Co");
        await dbContext.TaxEntities.AddAsync(taxEntity);
        await dbContext.SaveChangesAsync();

        company.UpdateTaxEntityId(taxEntity.Id);
        await dbContext.SaveChangesAsync();

        var user = User.Create("sales@local", "hash", "salt");
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        return (new CompanySeed(organization.Id, company.Id, company.PublicId), user.Id);
    }

    private static async Task<(CompanySeed CompanyA, CompanySeed CompanyB, long UserId)> SeedTwoCompaniesAsync(ErpDbContext dbContext)
    {
        var organization = Organization.Create(OrganizationType.Individual, "Sales Org");
        await dbContext.Organizations.AddAsync(organization);
        await dbContext.SaveChangesAsync();

        var companyA = Company.Create(organization.Id, 0, "Sales Company A");
        var companyB = Company.Create(organization.Id, 0, "Sales Company B");
        await dbContext.Companies.AddRangeAsync(companyA, companyB);
        await dbContext.SaveChangesAsync();

        var taxEntityA = TaxEntity.Create(companyA.PublicId, "SALESA", "Sales A");
        var taxEntityB = TaxEntity.Create(companyB.PublicId, "SALESB", "Sales B");
        await dbContext.TaxEntities.AddRangeAsync(taxEntityA, taxEntityB);
        await dbContext.SaveChangesAsync();

        companyA.UpdateTaxEntityId(taxEntityA.Id);
        companyB.UpdateTaxEntityId(taxEntityB.Id);
        await dbContext.SaveChangesAsync();

        var user = User.Create("sales@local", "hash", "salt");
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        return (
            new CompanySeed(organization.Id, companyA.Id, companyA.PublicId),
            new CompanySeed(organization.Id, companyB.Id, companyB.PublicId),
            user.Id);
    }

    private static async Task<Guid> GetOtherCompanyQuotePublicId(ApiWebApplicationFactory factory, long companyId)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
        return await dbContext.SalesQuotes
            .Where(quote => quote.CompanyId == companyId)
            .Select(quote => quote.PublicId)
            .SingleAsync();
    }

    private static async Task GrantSalesPermissionsAsync(ErpDbContext dbContext, Guid companyPublicId, long userId, params string[] permissions)
    {
        var role = Role.Create("Sales Quote Operator", scopeType: RoleAssignmentScopeType.Company);
        await dbContext.Roles.AddAsync(role);
        await dbContext.SaveChangesAsync();

        foreach (var permissionCode in permissions)
        {
            var permission = Permission.Create(permissionCode, permissionCode);
            await dbContext.Permissions.AddAsync(permission);
            await dbContext.SaveChangesAsync();
            await dbContext.RolePermissions.AddAsync(RolePermission.Create(role.Id, permission.Id));
        }

        var companyUser = CompanyUser.Create(companyPublicId, userId, CompanyUserStatus.Active);
        await dbContext.CompanyUsers.AddAsync(companyUser);
        await dbContext.RoleAssignments.AddAsync(RoleAssignment.CreateCompany(companyUser.UserId, role.Id, companyPublicId));
        await dbContext.SaveChangesAsync();
    }

    private sealed record CompanySeed(long OrganizationId, long CompanyId, Guid CompanyPublicId);
}
