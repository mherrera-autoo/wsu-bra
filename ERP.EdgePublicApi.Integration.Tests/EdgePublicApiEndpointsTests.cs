using System.Net;
using System.Net.Http.Json;
using ERP.Modules.Rfid.Domain;
using ERP.Modules.Wsu.Domain;
using ERP.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ERP.EdgePublicApi.Integration.Tests;

public sealed class EdgePublicApiEndpointsTests
{
    [Fact]
    public async Task EventsBatch_WithoutApiKey_Returns401()
    {
        using var factory = new EdgePublicApiWebApplicationFactory(Guid.NewGuid().ToString());
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/edge/events/batch", new
        {
            edgeNodeId = "EDGE-PLANTA-01",
            sentAtUtc = DateTimeOffset.UtcNow,
            events = Array.Empty<object>()
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task OrdersReady_WithoutApiKey_Returns401()
    {
        using var factory = new EdgePublicApiWebApplicationFactory(Guid.NewGuid().ToString());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/edge/orders/ready?edgeNodeId=EDGE-PLANTA-01");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task OrdersReady_InvalidApiKey_Returns401()
    {
        using var factory = new EdgePublicApiWebApplicationFactory(Guid.NewGuid().ToString());
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("x-api-key", "wrong-key");

        var response = await client.GetAsync("/api/v1/edge/orders/ready?edgeNodeId=EDGE-PLANTA-01");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task EventsBatch_MissingEdgeNodeId_Returns400()
    {
        using var factory = new EdgePublicApiWebApplicationFactory(Guid.NewGuid().ToString());
        using var client = CreateApiClient(factory);

        var response = await client.PostAsJsonAsync("/api/v1/edge/events/batch", new
        {
            edgeNodeId = "",
            sentAtUtc = DateTimeOffset.UtcNow,
            events = new[]
            {
                new
                {
                    eventId = Guid.NewGuid().ToString(),
                    eventType = "InventoryIngressDetected",
                    schemaVersion = 1,
                    occurredAtUtc = DateTimeOffset.UtcNow,
                    orderId = (Guid?)null,
                    epcHex = "3008abcd",
                    readerId = "RFID-GATE-01"
                }
            }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EventsBatch_InvalidEventItem_IsRejectedInBatchResult()
    {
        using var factory = new EdgePublicApiWebApplicationFactory(Guid.NewGuid().ToString());
        using var client = CreateApiClient(factory);

        var response = await client.PostAsJsonAsync("/api/v1/edge/events/batch", new
        {
            edgeNodeId = "EDGE-PLANTA-01",
            sentAtUtc = DateTimeOffset.UtcNow,
            events = new[]
            {
                new
                {
                    eventId = Guid.NewGuid().ToString(),
                    eventType = "InventoryIngressDetected",
                    schemaVersion = 1,
                    occurredAtUtc = DateTimeOffset.UtcNow,
                    orderId = (Guid?)null,
                    epcHex = "3008abcd",
                    readerId = "RFID-GATE-01"
                }
            }
        });

        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Rejected", content, StringComparison.Ordinal);
        Assert.Contains("orderId is required", content, StringComparison.Ordinal);
    }

    [Fact(Skip = "Requires PostgreSQL-backed integration database for full ErpDbContext model.")]
    public async Task EventsBatch_SameEventId_IsIdempotent()
    {
        using var factory = new EdgePublicApiWebApplicationFactory(Guid.NewGuid().ToString());
        using var client = CreateApiClient(factory);

        var eventId = Guid.NewGuid().ToString();
        var payload = new
        {
            edgeNodeId = "EDGE-PLANTA-01",
            sentAtUtc = DateTimeOffset.UtcNow,
            events = new[]
            {
                new
                {
                    eventId,
                    eventType = "InventoryEgressDetected",
                    schemaVersion = 1,
                    occurredAtUtc = DateTimeOffset.UtcNow,
                    orderId = Guid.NewGuid(),
                    epcHex = "3008abcd",
                    movementType = "ENTER",
                    readerId = "RFID-GATE-01"
                }
            }
        };

        var first = await client.PostAsJsonAsync("/api/v1/edge/events/batch", payload);
        var second = await client.PostAsJsonAsync("/api/v1/edge/events/batch", payload);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
        var count = dbContext.EdgeBusinessEvents.Count(item => item.EventId == eventId);
        Assert.Equal(1, count);
    }

    [Fact(Skip = "Requires PostgreSQL-backed integration database for full ErpDbContext model.")]
    public async Task OrdersActive_ReturnsActiveOrdersAndSupportsEtag304()
    {
        using var factory = new EdgePublicApiWebApplicationFactory(Guid.NewGuid().ToString());
        await SeedOrderDataAsync(factory.Services);

        using var client = CreateApiClient(factory);

        var firstResponse = await client.GetAsync("/api/v1/edge/orders/active?edgeNodeId=EDGE-PLANTA-01&readerId=RFID-GATE-01");
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var etag = firstResponse.Headers.ETag?.Tag;
        Assert.False(string.IsNullOrWhiteSpace(etag));

        var json = await firstResponse.Content.ReadAsStringAsync();
        Assert.Contains("ORD-EDGE-0001", json, StringComparison.Ordinal);
        Assert.Contains("3008ABCD", json, StringComparison.Ordinal);

        var secondRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/edge/orders/active?edgeNodeId=EDGE-PLANTA-01&readerId=RFID-GATE-01");
        secondRequest.Headers.TryAddWithoutValidation("If-None-Match", etag);

        var secondResponse = await client.SendAsync(secondRequest);
        Assert.Equal(HttpStatusCode.NotModified, secondResponse.StatusCode);
    }

    [Fact(Skip = "Requires PostgreSQL-backed integration database for full ErpDbContext model.")]
    public async Task OrdersReady_ReturnsOnlyOperationallyReadyOrders_WithUppercaseEpcs_And304Support()
    {
    }

    private static HttpClient CreateApiClient(EdgePublicApiWebApplicationFactory factory)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("x-api-key", "test-edge-api-key");
        return client;
    }

    private static async Task SeedOrderDataAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();

        var companyPublicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var warehousePublicId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var productPublicId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        var order = Order.Create(
            companyPublicId,
            warehousePublicId,
            OrderType.Inbound,
            "ORD-EDGE-0001",
            null,
            OrderStatus.Confirmed,
            DateTimeOffset.Parse("2026-04-21T18:20:00Z"),
            null,
            SourceType.Import,
            "edge test");

        await dbContext.WsuOrders.AddAsync(order);
        await dbContext.SaveChangesAsync();

        var item = OrderItem.Create(
            order.Id,
            productPublicId,
            quantityAvailable: 1,
            skuWsu: null,
            nombreSkuWsu: null,
            skuProveedor: null,
            nombreSkuProveedor: null,
            rutProveedor: null,
            rsProveedor: null,
            skuCliente: null,
            nombreSkuCliente: null,
            rutCliente: null,
            rsCliente: null,
            posicion: null,
            unidadDeMedida: null,
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
            variacionStock: 1,
            stockMinimo: null);

        await dbContext.WsuOrderItems.AddAsync(item);
        await dbContext.SaveChangesAsync();

        await dbContext.WsuOrderItemEpcAssignments.AddAsync(OrderItemEpcAssignment.Create(order.Id, item.Id, "3008abcd"));
        await dbContext.RfidTags.AddAsync(RfidTag.Create(companyPublicId, warehousePublicId, "3008abcd", null));
        await dbContext.SaveChangesAsync();
    }
}
