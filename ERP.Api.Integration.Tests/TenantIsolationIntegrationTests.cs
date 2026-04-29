using ERP.Api.Authorization;
using ERP.Api.Contracts.Sales;
using ERP.Modules.Identity.Domain;
using ERP.Modules.Inventory.Domain;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using ERP.Modules.Sales.Domain;
using ERP.Persistence;
using ERP.Shared.Application;
using ERP.Shared.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Xunit;

namespace ERP.Api.Integration.Tests;

public sealed class TenantIsolationIntegrationTests
{
    [Fact]
    public async Task InventoryStock_UsesTenantContextCompanyIdToPreventCrossCompanyLeakage()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long userId;
        CompanySeed companyA;
        CompanySeed companyB;
        long productAId;
        long productBId;
        long warehouseAId;
        long warehouseBId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var seed = await SeedTenantsAsync(dbContext);
            userId = seed.UserId;
            companyA = seed.CompanyA;
            companyB = seed.CompanyB;

            await GrantInventoryStockPermissionAsync(dbContext, companyA.CompanyPublicId, userId);

            var unit = UnitOfMeasure.Create("ea", "EA", UnitOfMeasureDimension.Count, true, 1m, 0, true, 1);
            await dbContext.UnitOfMeasures.AddAsync(unit);
            await dbContext.SaveChangesAsync();

            var productA = Product.Create(companyA.CompanyId, "SKU-A", "Product A", "780000000010", unit.Id, true, true, true);
            var productB = Product.Create(companyB.CompanyId, "SKU-B", "Product B", "780000000011", unit.Id, true, true, true);
            var warehouseA = Warehouse.Create(companyA.CompanyId, "WH-A", "Warehouse A");
            var warehouseB = Warehouse.Create(companyB.CompanyId, "WH-B", "Warehouse B");

            await dbContext.Products.AddRangeAsync(productA, productB);
            await dbContext.Warehouses.AddRangeAsync(warehouseA, warehouseB);
            await dbContext.SaveChangesAsync();

            var stockA = Stock.Create(companyA.CompanyId, productA.Id, warehouseA.Id, 15m);
            var stockB = Stock.Create(companyB.CompanyId, productB.Id, warehouseB.Id, 42m);
            await dbContext.Stocks.AddRangeAsync(stockA, stockB);
            await dbContext.SaveChangesAsync();

            productAId = productA.Id;
            productBId = productB.Id;
            warehouseAId = warehouseA.Id;
            warehouseBId = warehouseB.Id;
        }

        using var client = CreateClient(factory, companyA.OrganizationId, companyA.CompanyId, companyA.CompanyPublicId, userId);

        var ownStock = await client.GetFromJsonAsync<StockSnapshot>(
            $"/api/inventory/stock?productId={productAId}&warehouseId={warehouseAId}");

        Assert.NotNull(ownStock);
        Assert.Equal(companyA.CompanyId, ownStock!.CompanyId);
        Assert.Equal(15m, ownStock.OnHandQuantity);

        var otherStock = await client.GetFromJsonAsync<StockSnapshot>(
            $"/api/inventory/stock?productId={productBId}&warehouseId={warehouseBId}");

        Assert.NotNull(otherStock);
        Assert.Equal(companyA.CompanyId, otherStock!.CompanyId);
        Assert.Equal(0m, otherStock.OnHandQuantity); // ITenantContext CompanyId filter prevents cross-company leakage.
    }

    [Fact]
    public async Task SalesDocuments_RespectTenantContextCompanyId()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        long userId;
        CompanySeed companyA;
        CompanySeed companyB;
        long documentAId;
        long documentBId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var seed = await SeedTenantsAsync(dbContext);
            userId = seed.UserId;
            companyA = seed.CompanyA;
            companyB = seed.CompanyB;

            var unit = UnitOfMeasure.Create("ea", "EA", UnitOfMeasureDimension.Count, true, 1m, 0, true, 1);
            await dbContext.UnitOfMeasures.AddAsync(unit);
            await dbContext.SaveChangesAsync();

            var customerA = Customer.Create(companyA.CompanyId, "Customer A");
            var customerB = Customer.Create(companyB.CompanyId, "Customer B");
            var productA = Product.Create(companyA.CompanyId, "SKU-A", "Product A", "780000000012", unit.Id, true, true, true);
            var productB = Product.Create(companyB.CompanyId, "SKU-B", "Product B", "780000000013", unit.Id, true, true, true);

            await dbContext.Customers.AddRangeAsync(customerA, customerB);
            await dbContext.Products.AddRangeAsync(productA, productB);
            await dbContext.SaveChangesAsync();

            var documentA = SalesDocument.Create(companyA.CompanyId, SalesDocumentKind.Quote, customerA.Id);
            var documentB = SalesDocument.Create(companyB.CompanyId, SalesDocumentKind.Quote, customerB.Id);
            await dbContext.SalesDocuments.AddRangeAsync(documentA, documentB);
            await dbContext.SaveChangesAsync();

            documentA.AddLine(productA.Id, 2m, new Money(10m, "USD"), null, new Money(0m, "USD"));
            documentB.AddLine(productB.Id, 1m, new Money(20m, "USD"), null, new Money(0m, "USD"));
            await dbContext.SaveChangesAsync();

            documentAId = documentA.Id;
            documentBId = documentB.Id;
        }

        using var client = CreateClient(factory, companyA.OrganizationId, companyA.CompanyId, companyA.CompanyPublicId, userId);

        var documents = await client.GetFromJsonAsync<List<SalesDocumentSummary>>("/api/sales/documents");
        Assert.NotNull(documents);
        var document = Assert.Single(documents!);
        Assert.Equal(documentAId, document.Id);

        var crossTenantResponse = await client.GetAsync($"/api/sales/documents/{documentBId}");
        Assert.Equal(HttpStatusCode.NotFound, crossTenantResponse.StatusCode); // ITenantContext CompanyId filter protects document access.
    }

    private static async Task<SeedContext> SeedTenantsAsync(ErpDbContext dbContext)
    {
        var organization = Organization.Create(OrganizationType.Individual, "Tenant Org");
        await dbContext.Organizations.AddAsync(organization);
        await dbContext.SaveChangesAsync();

// Create companies first to get PublicIds
        var companyA = Company.Create(organization.Id, 0, "Tenant A");
        var companyB = Company.Create(organization.Id, 0, "Tenant B");
        await dbContext.Companies.AddRangeAsync(companyA, companyB);
        await dbContext.SaveChangesAsync();

        var taxEntityA = TaxEntity.Create(companyA.PublicId, "TENANT-A", "Tenant A");
        var taxEntityB = TaxEntity.Create(companyB.PublicId, "TENANT-B", "Tenant B");
        await dbContext.TaxEntities.AddRangeAsync(taxEntityA, taxEntityB);
        await dbContext.SaveChangesAsync();

// Update companies with correct TaxEntityIds
        companyA.UpdateTaxEntityId(taxEntityA.Id);
        companyB.UpdateTaxEntityId(taxEntityB.Id);
        await dbContext.SaveChangesAsync();

        var user = User.Create("tenant@local", "hash", "salt");
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        return new SeedContext(
            new CompanySeed(organization.Id, companyA.Id, companyA.PublicId),
            new CompanySeed(organization.Id, companyB.Id, companyB.PublicId),
            user.Id);
    }

    private static async Task GrantInventoryStockPermissionAsync(
        ErpDbContext dbContext,
        Guid companyPublicId,
        long userId)
    {
var permission = Permission.Create(PermissionKeys.Inventory.StockRead, "Inventory stock read");
        var role = Role.Create("Inventory Reader", scopeType: RoleAssignmentScopeType.Company);
        await dbContext.Permissions.AddAsync(permission);
        await dbContext.Roles.AddAsync(role);
        await dbContext.SaveChangesAsync();

        await dbContext.RolePermissions.AddAsync(RolePermission.Create(role.Id, permission.Id));
        var companyUser = CompanyUser.Create(companyPublicId, userId, CompanyUserStatus.Active);
        await dbContext.CompanyUsers.AddAsync(companyUser);
        await dbContext.SaveChangesAsync();

        await dbContext.RoleAssignments.AddAsync(RoleAssignment.CreateCompany(companyUser.UserId, role.Id, companyPublicId));
        await dbContext.SaveChangesAsync();
    }

    private static HttpClient CreateClient(
        ApiWebApplicationFactory factory,
        long organizationId,
        long companyId,
        Guid companyPublicId,
        long userId)
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

    private sealed record CompanySeed(long OrganizationId, long CompanyId, Guid CompanyPublicId);

    private sealed record SeedContext(CompanySeed CompanyA, CompanySeed CompanyB, long UserId);

    private sealed record StockSnapshot(long CompanyId, long ProductId, long WarehouseId, decimal OnHandQuantity);
}
