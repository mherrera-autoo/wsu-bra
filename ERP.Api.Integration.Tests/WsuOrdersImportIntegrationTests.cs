using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using ERP.Modules.Identity.Domain;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using ERP.Modules.Rfid.Domain;
using ERP.Modules.Wsu.Domain;
using ERP.Persistence;
using ERP.Shared.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ERP.Api.Integration.Tests;

public sealed class WsuOrdersImportIntegrationTests
{
    [Fact]
    public async Task ImportExcel_WhenSkuTripletIsDuplicated_ReturnsBadRequestWithRowError()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        CompanySeed company;
        long userId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (company, userId) = await SeedCompanyAsync(dbContext);
            await GrantWsuManagePermissionAsync(dbContext, userId);
        }

        using var client = CreateClient(factory, company, userId);
        using var content = new MultipartFormDataContent();

        var workbookBytes = BuildWorkbookWithDuplicatedTriplet();
        using var fileContent = new ByteArrayContent(workbookBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        content.Add(fileContent, "File", "wsu-import.xlsx");
        content.Add(new StringContent(((int)ERP.Modules.Wsu.Domain.OrderType.Inbound).ToString(CultureInfo.InvariantCulture), Encoding.UTF8), "OrderType");

        var response = await client.PostAsync("/api/wsu/orders/import", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseJson = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseJson);
        Assert.Equal("Invalid spreadsheet data.", document.RootElement.GetProperty("error").GetString());

        var rowErrors = document.RootElement.GetProperty("rowErrors");
        Assert.True(rowErrors.GetArrayLength() > 0);

        var duplicateError = rowErrors
            .EnumerateArray()
            .FirstOrDefault(item =>
                item.TryGetProperty("field", out var field)
                && string.Equals(field.GetString(), "SkuTriplet", StringComparison.Ordinal)
                && item.TryGetProperty("row", out var row)
                && row.GetInt32() == 3);

        Assert.NotEqual(JsonValueKind.Undefined, duplicateError.ValueKind);
        Assert.Contains("matches row 2", duplicateError.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ImportExcel_WhenSkuTripletsAreUnique_CreatesOrder()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        CompanySeed company;
        long userId;
        (Guid ProductA, Guid ProductB) productIds;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (company, userId) = await SeedCompanyAsync(dbContext);
            productIds = await SeedProductsAsync(dbContext, company.CompanyId);
            await GrantWsuManagePermissionAsync(dbContext, userId);
        }

        using var client = CreateClient(factory, company, userId);
        using var content = new MultipartFormDataContent();

        var workbookBytes = BuildWorkbookWithUniqueTriplets(productIds.ProductA, productIds.ProductB);
        using var fileContent = new ByteArrayContent(workbookBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        content.Add(fileContent, "File", "wsu-import.xlsx");
        content.Add(new StringContent(((int)ERP.Modules.Wsu.Domain.OrderType.Inbound).ToString(CultureInfo.InvariantCulture), Encoding.UTF8), "OrderType");

        var response = await client.PostAsync("/api/wsu/orders/import", content);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var responseJson = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseJson);
        Assert.Equal(2, document.RootElement.GetProperty("itemsCount").GetInt32());

        using var assertScope = factory.Services.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<ErpDbContext>();
        Assert.Equal(1, await assertDb.WsuOrders.CountAsync());
        Assert.Equal(2, await assertDb.WsuOrderItems.CountAsync());
    }

    [Fact]
    public async Task ImportEpcs_WhenEpcAlreadyExistsInSameCompanyAndWarehouse_ReturnsBadRequest()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        CompanySeed company;
        long userId;
        Guid orderPublicId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (company, userId) = await SeedCompanyAsync(dbContext);
            await GrantWsuManagePermissionAsync(dbContext, userId);

            var warehouse = await SeedWarehouseAsync(dbContext, company.CompanyId, "WH-001");
            var seededOrder = await SeedInboundOrderWithSingleItemAsync(dbContext, company.CompanyPublicId, warehouse.PublicId, "WSU-ORD-001");
            orderPublicId = seededOrder.PublicId;

            await dbContext.RfidTags.AddAsync(RfidTag.Create(company.CompanyPublicId, warehouse.PublicId, "EPC-EXISTENTE-001", "Pre-existing tag"));
            await dbContext.SaveChangesAsync();
        }

        using var client = CreateClient(factory, company, userId);
        using var content = new MultipartFormDataContent();

        var workbookBytes = BuildEpcWorkbook("EPC-EXISTENTE-001");
        using var fileContent = new ByteArrayContent(workbookBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        content.Add(fileContent, "File", "wsu-import-epc.xlsx");

        var response = await client.PostAsync($"/api/wsu/orders/{orderPublicId}/import-epc", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseJson = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseJson);
        var error = document.RootElement.GetProperty("error").GetString();
        Assert.NotNull(error);
        Assert.Contains("misma compañía y bodega", error, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("EPC-EXISTENTE-001", error, StringComparison.OrdinalIgnoreCase);

        using var assertScope = factory.Services.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<ErpDbContext>();
        Assert.Equal(0, await assertDb.WsuOrderItemEpcAssignments.CountAsync());
    }

    [Fact]
    public async Task ImportEpcs_WhenEpcExistsInDifferentCompany_AllowsImport()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        CompanySeed company;
        CompanySeed otherCompany;
        long userId;
        Guid orderPublicId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (company, userId) = await SeedCompanyAsync(dbContext);
            (otherCompany, _) = await SeedCompanyAsync(dbContext);
            await GrantWsuManagePermissionAsync(dbContext, userId);

            var warehouse = await SeedWarehouseAsync(dbContext, company.CompanyId, "WH-001");
            var seededOrder = await SeedInboundOrderWithSingleItemAsync(dbContext, company.CompanyPublicId, warehouse.PublicId, "WSU-ORD-002");
            orderPublicId = seededOrder.PublicId;

            await dbContext.RfidTags.AddAsync(RfidTag.Create(otherCompany.CompanyPublicId, warehouse.PublicId, "EPC-CROSS-COMPANY-001", "Different company tag"));
            await dbContext.SaveChangesAsync();
        }

        using var client = CreateClient(factory, company, userId);
        using var content = new MultipartFormDataContent();

        var workbookBytes = BuildEpcWorkbook("EPC-CROSS-COMPANY-001");
        using var fileContent = new ByteArrayContent(workbookBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        content.Add(fileContent, "File", "wsu-import-epc.xlsx");

        var response = await client.PostAsync($"/api/wsu/orders/{orderPublicId}/import-epc", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var assertScope = factory.Services.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<ErpDbContext>();
        Assert.Equal(1, await assertDb.WsuOrderItemEpcAssignments.CountAsync());
    }

    [Fact]
    public async Task ImportEpcs_WhenEpcExistsInDifferentWarehouse_AllowsImport()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        CompanySeed company;
        long userId;
        Guid orderPublicId;
        Guid secondaryWarehousePublicId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (company, userId) = await SeedCompanyAsync(dbContext);
            await GrantWsuManagePermissionAsync(dbContext, userId);

            var primaryWarehouse = await SeedWarehouseAsync(dbContext, company.CompanyId, "WH-001");
            var secondaryWarehouse = await SeedWarehouseAsync(dbContext, company.CompanyId, "WH-002");
            secondaryWarehousePublicId = secondaryWarehouse.PublicId;

            var seededOrder = await SeedInboundOrderWithSingleItemAsync(dbContext, company.CompanyPublicId, primaryWarehouse.PublicId, "WSU-ORD-003");
            orderPublicId = seededOrder.PublicId;

            await dbContext.RfidTags.AddAsync(RfidTag.Create(company.CompanyPublicId, secondaryWarehousePublicId, "EPC-CROSS-WH-001", "Different warehouse tag"));
            await dbContext.SaveChangesAsync();
        }

        using var client = CreateClient(factory, company, userId);
        using var content = new MultipartFormDataContent();

        var workbookBytes = BuildEpcWorkbook("EPC-CROSS-WH-001");
        using var fileContent = new ByteArrayContent(workbookBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        content.Add(fileContent, "File", "wsu-import-epc.xlsx");

        var response = await client.PostAsync($"/api/wsu/orders/{orderPublicId}/import-epc", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var assertScope = factory.Services.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<ErpDbContext>();
        Assert.Equal(1, await assertDb.WsuOrderItemEpcAssignments.CountAsync());
    }

    [Fact]
    public async Task CancelOrder_WhenEpcsWereLoaded_RemovesAssignmentsAndRfidTags_AndResetsEpcLoadFlag()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        CompanySeed company;
        long userId;
        Guid orderPublicId;
        Guid warehousePublicId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (company, userId) = await SeedCompanyAsync(dbContext);
            await GrantWsuManagePermissionAsync(dbContext, userId);

            var warehouse = await SeedWarehouseAsync(dbContext, company.CompanyId, "WH-001");
            warehousePublicId = warehouse.PublicId;

            var seededOrder = await SeedInboundOrderWithSingleItemAsync(
                dbContext,
                company.CompanyPublicId,
                warehousePublicId,
                "WSU-ORD-004",
                OrderStatus.Draft);
            orderPublicId = seededOrder.PublicId;
        }

        using var client = CreateClient(factory, company, userId);
        using var importContent = new MultipartFormDataContent();

        var workbookBytes = BuildEpcWorkbook("EPC-CANCEL-001");
        using var fileContent = new ByteArrayContent(workbookBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        importContent.Add(fileContent, "File", "wsu-import-epc.xlsx");

        var importResponse = await client.PostAsync($"/api/wsu/orders/{orderPublicId}/import-epc", importContent);
        Assert.Equal(HttpStatusCode.OK, importResponse.StatusCode);

        var cancelResponse = await client.PostAsJsonAsync($"/api/wsu/orders/{orderPublicId}/cancel", new
        {
            companyPublicId = company.CompanyPublicId
        });

        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);

        using var assertScope = factory.Services.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<ErpDbContext>();

        var order = await assertDb.WsuOrders.SingleAsync(item => item.PublicId == orderPublicId);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.False(order.IsEpcLoadConfirmed);

        var assignmentsCount = await assertDb.WsuOrderItemEpcAssignments.CountAsync(item => item.OrderId == order.Id);
        Assert.Equal(0, assignmentsCount);

        var tagsCount = await assertDb.RfidTags.CountAsync(item =>
            item.CompanyPublicId == company.CompanyPublicId
            && item.WarehousePublicId == warehousePublicId
            && item.Epc == "EPC-CANCEL-001");
        Assert.Equal(0, tagsCount);
    }

    [Fact]
    public async Task EpcChecks_WhenUpdatedInBulk_UpdatesOrderCompletionFlagInList()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        CompanySeed company;
        long userId;
        Guid orderPublicId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (company, userId) = await SeedCompanyAsync(dbContext);
            await GrantWsuManagePermissionAsync(dbContext, userId);

            var warehouse = await SeedWarehouseAsync(dbContext, company.CompanyId, "WH-CHK-001");
            var order = await SeedInboundOrderWithSingleItemAsync(
                dbContext,
                company.CompanyPublicId,
                warehouse.PublicId,
                "WSU-ORD-CHECK-001",
                OrderStatus.Draft);

            var orderItem = await dbContext.WsuOrderItems.SingleAsync(item => item.OrderId == order.Id);
            await dbContext.WsuOrderItemEpcAssignments.AddRangeAsync(
                OrderItemEpcAssignment.Create(order.Id, orderItem.Id, "EPC-CHECK-001"),
                OrderItemEpcAssignment.Create(order.Id, orderItem.Id, "EPC-CHECK-002"));
            await dbContext.SaveChangesAsync();

            orderPublicId = order.PublicId;
        }

        using var client = CreateClient(factory, company, userId);

        var completionInitial = await GetOrderListCompletionAsync(client, company.CompanyPublicId, orderPublicId);
        Assert.False(completionInitial);

        var patchOneResponse = await client.PatchAsJsonAsync($"/api/wsu/orders/{orderPublicId}/epc-checks", new
        {
            companyPublicId = company.CompanyPublicId,
            items = new[]
            {
                new { epc = "EPC-CHECK-001", isChecked = true }
            }
        });
        Assert.Equal(HttpStatusCode.OK, patchOneResponse.StatusCode);

        var patchOneJson = await patchOneResponse.Content.ReadAsStringAsync();
        using (var patchOneDoc = JsonDocument.Parse(patchOneJson))
        {
            Assert.False(patchOneDoc.RootElement.GetProperty("isEpcSkuMatchCompleted").GetBoolean());
            Assert.Equal(2, patchOneDoc.RootElement.GetProperty("totalAssignments").GetInt32());
            Assert.Equal(1, patchOneDoc.RootElement.GetProperty("checkedAssignments").GetInt32());
        }

        var completionAfterOne = await GetOrderListCompletionAsync(client, company.CompanyPublicId, orderPublicId);
        Assert.False(completionAfterOne);

        var patchAllResponse = await client.PatchAsJsonAsync($"/api/wsu/orders/{orderPublicId}/epc-checks", new
        {
            companyPublicId = company.CompanyPublicId,
            items = new[]
            {
                new { epc = "EPC-CHECK-001", isChecked = true },
                new { epc = "EPC-CHECK-002", isChecked = true }
            }
        });
        Assert.Equal(HttpStatusCode.OK, patchAllResponse.StatusCode);

        var patchAllJson = await patchAllResponse.Content.ReadAsStringAsync();
        using (var patchAllDoc = JsonDocument.Parse(patchAllJson))
        {
            Assert.True(patchAllDoc.RootElement.GetProperty("isEpcSkuMatchCompleted").GetBoolean());
            Assert.Equal(2, patchAllDoc.RootElement.GetProperty("checkedAssignments").GetInt32());
        }

        var completionAfterAll = await GetOrderListCompletionAsync(client, company.CompanyPublicId, orderPublicId);
        Assert.True(completionAfterAll);

        var patchDisableResponse = await client.PatchAsJsonAsync($"/api/wsu/orders/{orderPublicId}/epc-checks", new
        {
            companyPublicId = company.CompanyPublicId,
            items = new[]
            {
                new { epc = "EPC-CHECK-002", isChecked = false }
            }
        });
        Assert.Equal(HttpStatusCode.OK, patchDisableResponse.StatusCode);

        var completionAfterDisable = await GetOrderListCompletionAsync(client, company.CompanyPublicId, orderPublicId);
        Assert.False(completionAfterDisable);
    }

    [Fact]
    public async Task ListEpcAssignments_ReturnsJoinedOrderItemAndAssignmentFields()
    {
        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        CompanySeed company;
        long userId;
        Guid orderPublicId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (company, userId) = await SeedCompanyAsync(dbContext);
            await GrantWsuManagePermissionAsync(dbContext, userId);

            var warehouse = await SeedWarehouseAsync(dbContext, company.CompanyId, "WH-LIST-001");
            var order = await SeedInboundOrderWithSingleItemAsync(
                dbContext,
                company.CompanyPublicId,
                warehouse.PublicId,
                "WSU-ORD-LIST-001",
                OrderStatus.Draft);

            var orderItem = await dbContext.WsuOrderItems.SingleAsync(item => item.OrderId == order.Id);
            var checkedAssignment = OrderItemEpcAssignment.Create(order.Id, orderItem.Id, "EPC-LIST-001");
            checkedAssignment.SetChecked(true);
            var uncheckedAssignment = OrderItemEpcAssignment.Create(order.Id, orderItem.Id, "EPC-LIST-002");

            await dbContext.WsuOrderItemEpcAssignments.AddRangeAsync(checkedAssignment, uncheckedAssignment);
            await dbContext.SaveChangesAsync();

            orderPublicId = order.PublicId;
        }

        using var client = CreateClient(factory, company, userId);

        var response = await client.GetAsync($"/api/wsu/orders/{orderPublicId}/epc-assignments?companyPublicId={company.CompanyPublicId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseJson = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseJson);
        var items = document.RootElement.EnumerateArray().ToList();
        Assert.Equal(2, items.Count);

        var first = items[0];
        Assert.Equal("SKU-EPC", first.GetProperty("skuWsu").GetString());
        Assert.Equal("SKU EPC", first.GetProperty("nombreSkuWsu").GetString());
        Assert.True(first.GetProperty("skuProveedor").ValueKind is JsonValueKind.Null);
        Assert.True(first.GetProperty("nombreSkuProveedor").ValueKind is JsonValueKind.Null);
        Assert.True(first.GetProperty("skuCliente").ValueKind is JsonValueKind.Null);
        Assert.True(first.GetProperty("nombreSkuCliente").ValueKind is JsonValueKind.Null);
        Assert.Equal("EPC-LIST-001", first.GetProperty("epc").GetString());
        Assert.True(first.GetProperty("isChecked").GetBoolean());

        var second = items[1];
        Assert.Equal("EPC-LIST-002", second.GetProperty("epc").GetString());
        Assert.False(second.GetProperty("isChecked").GetBoolean());
    }

    private static async Task<bool> GetOrderListCompletionAsync(HttpClient client, Guid companyPublicId, Guid orderPublicId)
    {
        var response = await client.GetAsync($"/api/wsu/orders?companyPublicId={companyPublicId}&skip=0&take=100");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseJson = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseJson);

        var order = document.RootElement
            .GetProperty("items")
            .EnumerateArray()
            .First(item => item.GetProperty("publicId").GetGuid() == orderPublicId);

        return order.GetProperty("isEpcSkuMatchCompleted").GetBoolean();
    }

    private static byte[] BuildWorkbookWithDuplicatedTriplet()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("WSU_Import");

        worksheet.Cell(1, 1).Value = "SkuWsu";
        worksheet.Cell(1, 2).Value = "SkuProveedor";
        worksheet.Cell(1, 3).Value = "SkuCliente";
        worksheet.Cell(1, 4).Value = "VariacionStock";

        worksheet.Cell(2, 1).Value = "SKU-001";
        worksheet.Cell(2, 2).Value = "PROV-001";
        worksheet.Cell(2, 3).Value = "CLI-001";
        worksheet.Cell(2, 4).Value = 10m;

        worksheet.Cell(3, 1).Value = " sku-001 ";
        worksheet.Cell(3, 2).Value = "prov-001";
        worksheet.Cell(3, 3).Value = "cli-001";
        worksheet.Cell(3, 4).Value = 8m;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static byte[] BuildWorkbookWithUniqueTriplets(Guid productPublicIdA, Guid productPublicIdB)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("WSU_Import");

        worksheet.Cell(1, 1).Value = "ProductPublicId";
        worksheet.Cell(1, 2).Value = "SkuWsu";
        worksheet.Cell(1, 3).Value = "SkuProveedor";
        worksheet.Cell(1, 4).Value = "SkuCliente";
        worksheet.Cell(1, 5).Value = "VariacionStock";

        worksheet.Cell(2, 1).Value = productPublicIdA.ToString();
        worksheet.Cell(2, 2).Value = "SKU-001";
        worksheet.Cell(2, 3).Value = "PROV-001";
        worksheet.Cell(2, 4).Value = "CLI-001";
        worksheet.Cell(2, 5).Value = 10m;

        worksheet.Cell(3, 1).Value = productPublicIdB.ToString();
        worksheet.Cell(3, 2).Value = "SKU-002";
        worksheet.Cell(3, 3).Value = "PROV-001";
        worksheet.Cell(3, 4).Value = "CLI-001";
        worksheet.Cell(3, 5).Value = 8m;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static byte[] BuildEpcWorkbook(params string[] epcs)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("WSU_EpcImport");

        worksheet.Cell(1, 1).Value = "Epc";

        for (var index = 0; index < epcs.Length; index++)
        {
            worksheet.Cell(index + 2, 1).Value = epcs[index];
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static HttpClient CreateClient(ApiWebApplicationFactory factory, CompanySeed company, long userId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString(CultureInfo.InvariantCulture));
        client.DefaultRequestHeaders.Add(TestAuthHandler.OrganizationIdHeader, company.OrganizationId.ToString(CultureInfo.InvariantCulture));
        client.DefaultRequestHeaders.Add(TestAuthHandler.ScopeHeader, "tenant");
        client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyIdHeader, company.CompanyId.ToString(CultureInfo.InvariantCulture));
        client.DefaultRequestHeaders.Add(TestAuthHandler.CompanyPublicIdHeader, company.CompanyPublicId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, string.Empty);
        return client;
    }

    private static async Task<(CompanySeed Company, long UserId)> SeedCompanyAsync(ErpDbContext dbContext)
    {
        var organization = Organization.Create(OrganizationType.Individual, "WSU Org");
        await dbContext.Organizations.AddAsync(organization);
        await dbContext.SaveChangesAsync();

        var company = Company.Create(organization.Id, 0, "WSU Company");
        await dbContext.Companies.AddAsync(company);
        await dbContext.SaveChangesAsync();

        var taxEntity = TaxEntity.Create(company.PublicId, "WSU-TAX", "WSU Company");
        await dbContext.TaxEntities.AddAsync(taxEntity);
        await dbContext.SaveChangesAsync();

        company.UpdateTaxEntityId(taxEntity.Id);
        await dbContext.SaveChangesAsync();

        var user = User.Create("wsu@test.local", "hash", "salt");
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        return (new CompanySeed(organization.Id, company.Id, company.PublicId), user.Id);
    }

    private static async Task GrantWsuManagePermissionAsync(ErpDbContext dbContext, long userId)
    {
        var role = Role.Create("WSU Platform Operator", scopeType: RoleAssignmentScopeType.Platform);
        await dbContext.Roles.AddAsync(role);
        await dbContext.SaveChangesAsync();

        var permission = Permission.Create(PermissionKeys.Platform.WsuManage, PermissionKeys.Platform.WsuManage);
        await dbContext.Permissions.AddAsync(permission);
        await dbContext.SaveChangesAsync();

        await dbContext.RolePermissions.AddAsync(RolePermission.Create(role.Id, permission.Id));
        await dbContext.RoleAssignments.AddAsync(RoleAssignment.CreatePlatform(userId, role.Id));
        await dbContext.SaveChangesAsync();
    }

    private static async Task<(Guid ProductA, Guid ProductB)> SeedProductsAsync(ErpDbContext dbContext, long companyId)
    {
        var unit = UnitOfMeasure.Create("ea", "EA", UnitOfMeasureDimension.Count, true, 1m, 0, true, null);
        await dbContext.UnitOfMeasures.AddAsync(unit);
        await dbContext.SaveChangesAsync();

        var productA = Product.Create(companyId, "SKU-001", "Product A", null, unit.Id, true, true, true);
        var productB = Product.Create(companyId, "SKU-002", "Product B", null, unit.Id, true, true, true);
        await dbContext.Products.AddRangeAsync(productA, productB);
        await dbContext.SaveChangesAsync();

        return (productA.PublicId, productB.PublicId);
    }

    private static async Task<Warehouse> SeedWarehouseAsync(ErpDbContext dbContext, long companyId, string code)
    {
        var warehouse = Warehouse.Create(companyId, code, code, "Main location");
        await dbContext.Warehouses.AddAsync(warehouse);
        await dbContext.SaveChangesAsync();
        return warehouse;
    }

    private static async Task<Order> SeedInboundOrderWithSingleItemAsync(
        ErpDbContext dbContext,
        Guid companyPublicId,
        Guid warehousePublicId,
        string orderNumber,
        OrderStatus status = OrderStatus.Confirmed)
    {
        var order = Order.Create(
            companyPublicId,
            warehousePublicId,
            OrderType.Inbound,
            orderNumber,
            externalOrderNumber: null,
            status: status,
            movementDate: DateTimeOffset.UtcNow,
            createdByUserPublicId: null,
            sourceType: SourceType.Import,
            notes: null);

        await dbContext.WsuOrders.AddAsync(order);
        await dbContext.SaveChangesAsync();

        var item = OrderItem.Create(
            order.Id,
            Guid.NewGuid(),
            quantityAvailable: 0m,
            skuWsu: "SKU-EPC",
            nombreSkuWsu: "SKU EPC",
            skuProveedor: null,
            nombreSkuProveedor: null,
            rutProveedor: null,
            rsProveedor: null,
            skuCliente: null,
            nombreSkuCliente: null,
            rutCliente: null,
            rsCliente: null,
            posicion: null,
            unidadDeMedida: "EA",
            loteMinimoCompra: null,
            largoCompraCm: null,
            anchoCompraCm: null,
            altoCompraCm: null,
            pesoCompraKg: null,
            apilableCompra: null,
            tipoAlmacenamientoCompra: null,
            loteMinimoVenta: null,
            largoVentaCm: null,
            anchoVentaCm: null,
            altoVentaCm: null,
            pesoVentaKg: null,
            apilableVenta: null,
            tipoAlmacenamientoVenta: null,
            precioCompraUnitario: null,
            precioVentaUnitario: null,
            variacionStock: 1m,
            stockMinimo: null);

        await dbContext.WsuOrderItems.AddAsync(item);
        await dbContext.SaveChangesAsync();

        return order;
    }

    private sealed record CompanySeed(long OrganizationId, long CompanyId, Guid CompanyPublicId);
}
