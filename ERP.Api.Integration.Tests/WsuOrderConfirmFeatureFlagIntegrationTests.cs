using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ERP.Api.Authorization;
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

public sealed class WsuOrderConfirmFeatureFlagIntegrationTests
{
    [Fact]
    public async Task ConfirmOrder_WhenFlagIsFalse_AllowsConfirmationWithoutOperatorRfidEnrollment()
    {
        using var factory = CreateFactory(validarOperadorEnOrden: false);
        CompanySeed company;
        long userId;
        Guid productPublicId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (company, userId) = await SeedCompanyAsync(dbContext);
            productPublicId = await SeedProductAsync(dbContext, company.CompanyId);
            await GrantWsuManagePermissionAsync(dbContext, userId);
            await SeedOperatorAsync(dbContext, company.CompanyPublicId, "OP-001", withValidCredential: false);
        }

        using var client = CreateClient(factory, company, userId);
        var orderPublicId = await CreateDraftInboundOrderAsync(client, company.CompanyPublicId, productPublicId);

        var response = await client.PostAsJsonAsync($"/api/wsu/orders/{orderPublicId}/confirm", new
        {
            companyPublicId = company.CompanyPublicId
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var assertScope = factory.Services.CreateScope();
        var assertDbContext = assertScope.ServiceProvider.GetRequiredService<ErpDbContext>();
        var order = await assertDbContext.WsuOrders.SingleAsync(item => item.PublicId == orderPublicId);
        Assert.Equal(ERP.Modules.Wsu.Domain.OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public async Task ConfirmOrder_WhenFlagIsTrue_ReturnsErrorWhenOrderHasNoAssignedOperators()
    {
        using var factory = CreateFactory(validarOperadorEnOrden: true);
        CompanySeed company;
        long userId;
        Guid productPublicId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (company, userId) = await SeedCompanyAsync(dbContext);
            productPublicId = await SeedProductAsync(dbContext, company.CompanyId);
            await GrantWsuManagePermissionAsync(dbContext, userId);
            await SeedOperatorAsync(dbContext, company.CompanyPublicId, "OP-001", withValidCredential: false);
        }

        using var client = CreateClient(factory, company, userId);
        var orderPublicId = await CreateDraftInboundOrderAsync(client, company.CompanyPublicId, productPublicId);

        var response = await client.PostAsJsonAsync($"/api/wsu/orders/{orderPublicId}/confirm", new
        {
            companyPublicId = company.CompanyPublicId
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseJson = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseJson);
        Assert.Equal(
            "At least one operator must be associated with the order before confirming this order.",
            document.RootElement.GetProperty("error").GetString());
    }

    [Fact]
    public async Task ConfirmOrder_WhenFlagIsTrue_AndOperatorHasValidRfidCredential_ConfirmsOrder()
    {
        using var factory = CreateFactory(validarOperadorEnOrden: true);
        CompanySeed company;
        long userId;
        Guid productPublicId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (company, userId) = await SeedCompanyAsync(dbContext);
            productPublicId = await SeedProductAsync(dbContext, company.CompanyId);
            await GrantWsuManagePermissionAsync(dbContext, userId);
            await SeedOperatorAsync(dbContext, company.CompanyPublicId, "OP-001", withValidCredential: true);
        }

        using var client = CreateClient(factory, company, userId);
        var orderPublicId = await CreateDraftInboundOrderAsync(client, company.CompanyPublicId, productPublicId);

        var assignOperatorsResponse = await AssignMovementOperatorsAsync(client, company.CompanyPublicId, orderPublicId, "OP-001");
        Assert.Equal(HttpStatusCode.OK, assignOperatorsResponse.StatusCode);

        var response = await client.PostAsJsonAsync($"/api/wsu/orders/{orderPublicId}/confirm", new
        {
            companyPublicId = company.CompanyPublicId
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var assertScope = factory.Services.CreateScope();
        var assertDbContext = assertScope.ServiceProvider.GetRequiredService<ErpDbContext>();
        var order = await assertDbContext.WsuOrders.SingleAsync(item => item.PublicId == orderPublicId);
        Assert.Equal(ERP.Modules.Wsu.Domain.OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public async Task AssignMovementOperators_WhenOrderHasNoPhysicalMovements_ReturnsOk()
    {
        using var factory = CreateFactory(validarOperadorEnOrden: false);
        CompanySeed company;
        long userId;
        Guid productPublicId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (company, userId) = await SeedCompanyAsync(dbContext);
            productPublicId = await SeedProductAsync(dbContext, company.CompanyId);
            await GrantWsuManagePermissionAsync(dbContext, userId);
        }

        using var client = CreateClient(factory, company, userId);
        var orderPublicId = await CreateDraftInboundOrderAsync(client, company.CompanyPublicId, productPublicId);

        var response = await AssignMovementOperatorsAsync(client, company.CompanyPublicId, orderPublicId, "OP-001");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var assertScope = factory.Services.CreateScope();
        var assertDbContext = assertScope.ServiceProvider.GetRequiredService<ErpDbContext>();
        var order = await assertDbContext.WsuOrders.SingleAsync(item => item.PublicId == orderPublicId);
        var assignedCodes = await assertDbContext.WsuOrderMovementOperators
            .Where(item => item.OrderId == order.Id)
            .Select(item => item.CodigoOperador)
            .ToListAsync();

        Assert.True(order.IsMovementProgrammed);
        Assert.Single(assignedCodes);
        Assert.Equal("OP-001", assignedCodes[0]);
    }

    [Fact]
    public async Task AssignMovementOperators_WhenCalledTwice_ReplacesExistingOperators()
    {
        using var factory = CreateFactory(validarOperadorEnOrden: false);
        CompanySeed company;
        long userId;
        Guid productPublicId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            (company, userId) = await SeedCompanyAsync(dbContext);
            productPublicId = await SeedProductAsync(dbContext, company.CompanyId);
            await GrantWsuManagePermissionAsync(dbContext, userId);
        }

        using var client = CreateClient(factory, company, userId);
        var orderPublicId = await CreateDraftInboundOrderAsync(client, company.CompanyPublicId, productPublicId);

        var firstAssign = await AssignMovementOperatorsAsync(client, company.CompanyPublicId, orderPublicId, "OP-001", "OP-002");
        Assert.Equal(HttpStatusCode.OK, firstAssign.StatusCode);

        var secondAssign = await AssignMovementOperatorsAsync(client, company.CompanyPublicId, orderPublicId, "OP-003");
        Assert.Equal(HttpStatusCode.OK, secondAssign.StatusCode);

        using var assertScope = factory.Services.CreateScope();
        var assertDbContext = assertScope.ServiceProvider.GetRequiredService<ErpDbContext>();

        var order = await assertDbContext.WsuOrders.SingleAsync(item => item.PublicId == orderPublicId);
        var assignedCodes = await assertDbContext.WsuOrderMovementOperators
            .Where(item => item.OrderId == order.Id)
            .Select(item => item.CodigoOperador)
            .ToListAsync();

        Assert.True(order.IsMovementProgrammed);
        Assert.Single(assignedCodes);
        Assert.Equal("OP-003", assignedCodes[0]);
    }

    private static ApiWebApplicationFactory CreateFactory(bool validarOperadorEnOrden)
        => new(
            Guid.NewGuid().ToString(),
            new Dictionary<string, string?>
            {
                ["Wsu:ValidarOperadorEnOrden"] = validarOperadorEnOrden ? "true" : "false"
            });

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

    private static async Task<Guid> CreateDraftInboundOrderAsync(HttpClient client, Guid companyPublicId, Guid productPublicId)
    {
        var response = await client.PostAsJsonAsync("/api/wsu/orders", new
        {
            companyPublicId,
            orderType = 1,
            orderNumber = $"WSU-FF-{Guid.NewGuid():N}",
            status = 1,
            sourceType = 1,
            items = new[]
            {
                new
                {
                    productPublicId,
                    skuWsu = "SKU-001",
                    nombreSkuWsu = "SKU 001",
                    variacionStock = 5m,
                    unidadDeMedida = "EA"
                }
            }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var responseJson = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseJson);
        return document.RootElement.GetProperty("publicId").GetGuid();
    }

    private static Task<HttpResponseMessage> AssignMovementOperatorsAsync(HttpClient client, Guid companyPublicId, Guid orderPublicId, params string[] operatorCodes)
        => client.PostAsJsonAsync($"/api/wsu/orders/{orderPublicId}/movement-operators", new
        {
            companyPublicId,
            items = operatorCodes.Select(code => new
            {
                codigoOperador = code,
                nombreOperador = $"Operator {code}"
            }).ToArray()
        });

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

        var user = User.Create("wsu-confirm@test.local", "hash", "salt");
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

    private static async Task<Guid> SeedProductAsync(ErpDbContext dbContext, long companyId)
    {
        var unit = UnitOfMeasure.Create("ea", "EA", UnitOfMeasureDimension.Count, true, 1m, 0, true, null);
        await dbContext.UnitOfMeasures.AddAsync(unit);
        await dbContext.SaveChangesAsync();

        var product = Product.Create(companyId, "SKU-001", "Product A", null, unit.Id, true, true, true);
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        return product.PublicId;
    }

    private static async Task SeedOperatorAsync(ErpDbContext dbContext, Guid companyPublicId, string operatorCode, bool withValidCredential)
    {
        var op = Operator.Create(companyPublicId, operatorCode, "Operator Test", "11.111.111-1", isActive: true);
        await dbContext.Operators.AddAsync(op);
        await dbContext.SaveChangesAsync();

        if (!withValidCredential)
        {
            return;
        }

        var credential = OperatorCredential.Create(
            companyPublicId,
            op.Id,
            faceTemplateId: "FACE-001",
            nfcCardUid: "NFC-001",
            isActive: true);

        await dbContext.OperatorCredentials.AddAsync(credential);
        await dbContext.SaveChangesAsync();
    }

    private sealed record CompanySeed(long OrganizationId, long CompanyId, Guid CompanyPublicId);
}
