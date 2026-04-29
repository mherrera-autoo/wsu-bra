using System.Security.Cryptography;
using System.Text;
using ERP.EdgePublicApi.Configuration;
using ERP.EdgePublicApi.Contracts.Edge;
using ERP.EdgePublicApi.Middleware;
using ERP.EdgePublicApi.Services;
using ERP.Modules.Wsu.Domain;
using ERP.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ERP.EdgePublicApi.Controllers;

[ApiController]
[Route("api/v1/edge")]
public sealed class EdgeV1Controller : ControllerBase
{
    private sealed record ReadyOrderSnapshot(
        long Id,
        Guid PublicId,
        string OrderNumber,
        OrderType OrderType,
        DateTimeOffset MovementDate,
        bool IsEpcLoadConfirmed,
        bool IsEpcSkuMatchCompleted,
        bool IsMovementProgrammed,
        DateTime CreatedAt,
        DateTime? UpdatedAt);

    private sealed record ReadyOrderEpcSnapshot(
        long OrderId,
        string Epc,
        DateTime CreatedAt,
        DateTime? UpdatedAt);

    private readonly ErpDbContext _dbContext;
    private readonly EdgeEventIngestionService _ingestionService;
    private readonly IOptionsMonitor<EdgePublicApiOptions> _optionsMonitor;
    private readonly ILogger<EdgeV1Controller> _logger;

    public EdgeV1Controller(
        ErpDbContext dbContext,
        EdgeEventIngestionService ingestionService,
        IOptionsMonitor<EdgePublicApiOptions> optionsMonitor,
        ILogger<EdgeV1Controller> logger)
    {
        _dbContext = dbContext;
        _ingestionService = ingestionService;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    [HttpPost("events/batch")]
    [ProducesResponseType(typeof(EdgeEventsBatchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EdgeApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(EdgeApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(EdgeApiErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(EdgeApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> IngestBatch([FromBody] EdgeEventsBatchRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.EdgeNodeId))
        {
            return ErrorResponses.BadRequest(HttpContext, "invalid_payload", "edgeNodeId is required.");
        }

        if (!_optionsMonitor.CurrentValue.TryGetNode(request.EdgeNodeId, out var edgeNode))
        {
            return ErrorResponses.BadRequest(HttpContext, "unknown_edge_node", "edgeNodeId is not configured.");
        }

        if (request.Events is null || request.Events.Count == 0)
        {
            return ErrorResponses.BadRequest(HttpContext, "invalid_payload", "events must contain at least one item.");
        }

        var results = new List<EdgeBatchResultItem>(request.Events.Count);
        var accepted = 0;
        var rejected = 0;

        foreach (var edgeEvent in request.Events)
        {
            var validationError = ValidateEvent(edgeEvent);
            if (validationError is not null)
            {
                rejected++;
                results.Add(new EdgeBatchResultItem(edgeEvent.EventId, "Rejected", validationError));
                _logger.LogWarning(
                    "Edge event rejected edgeNodeId={EdgeNodeId} eventId={EventId} eventType={EventType} reason={Reason}",
                    request.EdgeNodeId,
                    edgeEvent.EventId,
                    edgeEvent.EventType,
                    validationError);
                continue;
            }

            var ingestionResult = await _ingestionService.IngestAsync(
                edgeNode.CompanyPublicId,
                request.EdgeNodeId.Trim(),
                edgeEvent,
                cancellationToken);

            if (string.Equals(ingestionResult.Status, "Accepted", StringComparison.Ordinal))
            {
                accepted++;
            }
            else
            {
                rejected++;
            }

            results.Add(ingestionResult);
        }

        return Ok(new EdgeEventsBatchResponse(accepted, rejected, results));
    }

    [HttpGet("orders/active")]
    [ProducesResponseType(typeof(EdgeActiveOrdersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(typeof(EdgeApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(EdgeApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(EdgeApiErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(EdgeApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetActiveOrders([FromQuery] string edgeNodeId, [FromQuery] string? readerId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(edgeNodeId))
        {
            return ErrorResponses.BadRequest(HttpContext, "invalid_payload", "edgeNodeId is required.");
        }

        var options = _optionsMonitor.CurrentValue;
        if (!options.TryGetNode(edgeNodeId, out var edgeNode))
        {
            return ErrorResponses.BadRequest(HttpContext, "unknown_edge_node", "edgeNodeId is not configured.");
        }

        if (!string.IsNullOrWhiteSpace(readerId)
            && edgeNode.ReaderIds.Length > 0
            && !edgeNode.ReaderIds.Contains(readerId.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            return Ok(new EdgeActiveOrdersResponse("empty", DateTimeOffset.UtcNow, []));
        }

        var query = _dbContext.WsuOrders
            .AsNoTracking()
            .Include(order => order.Items)
            .Where(order => order.CompanyPublicId == edgeNode.CompanyPublicId)
            .Where(order => order.Status == OrderStatus.Confirmed);

        if (edgeNode.WarehousePublicId.HasValue)
        {
            query = query.Where(order => order.WarehousePublicId == edgeNode.WarehousePublicId.Value);
        }

        var orders = await query
            .OrderBy(order => order.MovementDate)
            .ThenBy(order => order.Id)
            .ToListAsync(cancellationToken);

        var orderItemIds = orders
            .SelectMany(order => order.Items)
            .Select(item => item.Id)
            .Distinct()
            .ToArray();

        var assignments = orderItemIds.Length == 0
            ? []
            : await _dbContext.WsuOrderItemEpcAssignments
                .AsNoTracking()
                .Where(item => orderItemIds.Contains(item.OrderItemId))
                .Select(item => new { item.OrderItemId, item.Epc })
                .ToListAsync(cancellationToken);

        var epcByOrderItem = assignments
            .GroupBy(item => item.OrderItemId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group
                    .Select(item => item.Epc.Trim().ToUpperInvariant())
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(item => item, StringComparer.Ordinal)
                    .ToArray());

        var projectedOrders = orders
            .Select(order =>
            {
                var epcHexValues = order.Items
                    .SelectMany(item => epcByOrderItem.GetValueOrDefault(item.Id) ?? [])
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(item => item, StringComparer.Ordinal)
                    .ToArray();

                return new EdgeActiveOrderResponse(
                    order.OrderNumber,
                    "Active",
                    edgeNode.ValidFromUtc ?? order.MovementDate,
                    edgeNode.ValidToUtc ?? DateTimeOffset.UtcNow.AddHours(12),
                    edgeNode.AllowedZones,
                    edgeNode.MovementPolicy ?? "Default",
                    epcHexValues.Select(epc => new EdgeOrderItemResponse(epc)).ToArray());
            })
            .ToArray();

        var snapshotToken = BuildSnapshotToken(edgeNodeId, readerId, orders, projectedOrders);
        var etag = $"\"{snapshotToken}\"";

        if (Request.Headers.TryGetValue("If-None-Match", out var ifNoneMatch)
            && string.Equals(ifNoneMatch.ToString(), etag, StringComparison.Ordinal))
        {
            Response.Headers.ETag = etag;
            return StatusCode(StatusCodes.Status304NotModified);
        }

        Response.Headers.ETag = etag;
        return Ok(new EdgeActiveOrdersResponse(snapshotToken, DateTimeOffset.UtcNow, projectedOrders));
    }

    [HttpGet("orders/ready")]
    [ProducesResponseType(typeof(EdgeReadyOrdersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(typeof(EdgeApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(EdgeApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(EdgeApiErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(EdgeApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetReadyOrders([FromQuery] string edgeNodeId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(edgeNodeId))
        {
            return ErrorResponses.BadRequest(HttpContext, "invalid_payload", "edgeNodeId is required.");
        }

        var options = _optionsMonitor.CurrentValue;
        if (!options.TryGetNode(edgeNodeId, out var edgeNode))
        {
            return ErrorResponses.BadRequest(HttpContext, "unknown_edge_node", "edgeNodeId is not configured.");
        }

        var ordersQuery = _dbContext.WsuOrders
            .AsNoTracking()
            .Where(order => order.CompanyPublicId == edgeNode.CompanyPublicId)
            .Where(order => order.Status == OrderStatus.Confirmed)
            .Where(order => order.IsEpcLoadConfirmed)
            .Where(order => order.IsEpcSkuMatchCompleted)
            .Where(order => order.IsMovementProgrammed);

        if (edgeNode.WarehousePublicId.HasValue)
        {
            ordersQuery = ordersQuery.Where(order => order.WarehousePublicId == edgeNode.WarehousePublicId.Value);
        }

        var orders = await ordersQuery
            .OrderBy(order => order.MovementDate)
            .ThenBy(order => order.Id)
            .Select(order => new ReadyOrderSnapshot(
                order.Id,
                order.PublicId,
                order.OrderNumber,
                order.OrderType,
                order.MovementDate,
                order.IsEpcLoadConfirmed,
                order.IsEpcSkuMatchCompleted,
                order.IsMovementProgrammed,
                order.CreatedAt,
                order.UpdatedAt))
            .ToListAsync(cancellationToken);

        if (orders.Count == 0)
        {
            const string emptyVersion = "empty";
            var emptyEtag = $"\"{emptyVersion}\"";

            if (Request.Headers.TryGetValue("If-None-Match", out var emptyIfNoneMatch)
                && string.Equals(emptyIfNoneMatch.ToString(), emptyEtag, StringComparison.Ordinal))
            {
                Response.Headers.ETag = emptyEtag;
                return StatusCode(StatusCodes.Status304NotModified);
            }

            Response.Headers.ETag = emptyEtag;
            return Ok(new EdgeReadyOrdersResponse(emptyVersion, DateTimeOffset.UtcNow, []));
        }

        var orderIds = orders.Select(order => order.Id).ToArray();

        var epcsByOrder = await _dbContext.WsuOrderItemEpcAssignments
            .AsNoTracking()
            .Where(assignment => orderIds.Contains(assignment.OrderId))
            .Select(assignment => new ReadyOrderEpcSnapshot(
                assignment.OrderId,
                assignment.Epc,
                assignment.CreatedAt,
                assignment.UpdatedAt))
            .ToListAsync(cancellationToken);

        var groupedEpcs = epcsByOrder
            .GroupBy(item => item.OrderId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group
                    .Select(item => item.Epc.Trim().ToUpperInvariant())
                    .Where(item => !string.IsNullOrWhiteSpace(item))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(item => item, StringComparer.Ordinal)
                    .ToArray());

        var projected = orders
            .Select(order => new EdgeReadyOrderResponse(
                order.PublicId.ToString(),
                order.OrderNumber,
                order.OrderType.ToString(),
                groupedEpcs.GetValueOrDefault(order.Id) ?? []))
            .ToArray();

        var version = BuildReadySnapshotToken(edgeNodeId, orders, epcsByOrder);
        var etag = $"\"{version}\"";

        if (Request.Headers.TryGetValue("If-None-Match", out var ifNoneMatch)
            && string.Equals(ifNoneMatch.ToString(), etag, StringComparison.Ordinal))
        {
            Response.Headers.ETag = etag;
            return StatusCode(StatusCodes.Status304NotModified);
        }

        Response.Headers.ETag = etag;
        return Ok(new EdgeReadyOrdersResponse(version, DateTimeOffset.UtcNow, projected));
    }

    private static string? ValidateEvent(EdgeBusinessEventRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EventId))
        {
            return "eventId is required.";
        }

        if (!Guid.TryParse(request.EventId, out _))
        {
            return "eventId must be a UUID.";
        }

        if (string.IsNullOrWhiteSpace(request.EventType))
        {
            return "eventType is required.";
        }

        if (!EdgeEventTypes.IsAllowed(request.EventType.Trim()))
        {
            return "eventType is not supported.";
        }

        if (request.OccurredAtUtc == default)
        {
            return "occurredAtUtc is required.";
        }

        if (string.IsNullOrWhiteSpace(request.EpcHex))
        {
            return "epcHex is required.";
        }

        if (string.IsNullOrWhiteSpace(request.ReaderId))
        {
            return "readerId is required.";
        }

        if (!request.OrderId.HasValue || request.OrderId.Value == Guid.Empty)
        {
            return "orderId is required.";
        }

        return null;
    }

    private static string BuildSnapshotToken(
        string edgeNodeId,
        string? readerId,
        IReadOnlyList<Order> orders,
        IReadOnlyList<EdgeActiveOrderResponse> projectedOrders)
    {
        var sb = new StringBuilder();
        sb.Append(edgeNodeId.Trim()).Append('|').Append(readerId?.Trim() ?? string.Empty).Append('|');

        foreach (var order in orders)
        {
            sb.Append(order.PublicId).Append('|');
            sb.Append(order.UpdatedAt?.Ticks ?? order.CreatedAt.Ticks).Append('|');
            foreach (var item in order.Items.OrderBy(item => item.Id))
            {
                sb.Append(item.PublicId).Append('|').Append(item.UpdatedAt?.Ticks ?? item.CreatedAt.Ticks).Append('|');
            }
        }

        foreach (var projectedOrder in projectedOrders)
        {
            sb.Append(projectedOrder.OrderId).Append('|');
            foreach (var item in projectedOrder.Items)
            {
                sb.Append(item.EpcHex).Append('|');
            }
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string BuildReadySnapshotToken(
        string edgeNodeId,
        IReadOnlyList<ReadyOrderSnapshot> orders,
        IReadOnlyList<ReadyOrderEpcSnapshot> epcsByOrder)
    {
        var sb = new StringBuilder();
        sb.Append(edgeNodeId.Trim()).Append('|');

        foreach (var order in orders.OrderBy(order => order.Id))
        {
            sb.Append(order.Id).Append('|');
            sb.Append(order.PublicId).Append('|');
            sb.Append(order.OrderNumber).Append('|');
            sb.Append(order.OrderType).Append('|');
            sb.Append(order.MovementDate).Append('|');
            sb.Append(order.IsEpcLoadConfirmed).Append('|');
            sb.Append(order.IsEpcSkuMatchCompleted).Append('|');
            sb.Append(order.IsMovementProgrammed).Append('|');
            sb.Append(order.UpdatedAt?.Ticks ?? order.CreatedAt.Ticks).Append('|');
        }

        foreach (var epc in epcsByOrder
                     .OrderBy(item => item.OrderId)
                     .ThenBy(item => item.Epc.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            var normalized = epc.Epc.Trim().ToUpperInvariant();
            sb.Append(epc.OrderId).Append('|');
            sb.Append(normalized).Append('|');
            sb.Append(epc.UpdatedAt?.Ticks ?? epc.CreatedAt.Ticks).Append('|');
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
