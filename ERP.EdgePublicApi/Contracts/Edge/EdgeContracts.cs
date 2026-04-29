namespace ERP.EdgePublicApi.Contracts.Edge;

public static class EdgeEventTypes
{
    public const string InventoryIngressDetected = "InventoryIngressDetected";
    public const string InventoryEgressDetected = "InventoryEgressDetected";

    public static bool IsAllowed(string eventType)
        => string.Equals(eventType, InventoryIngressDetected, StringComparison.Ordinal)
           || string.Equals(eventType, InventoryEgressDetected, StringComparison.Ordinal);
}

public sealed class EdgeEventsBatchRequest
{
    public string EdgeNodeId { get; init; } = string.Empty;
    public DateTimeOffset SentAtUtc { get; init; }
    public IReadOnlyList<EdgeBusinessEventRequest> Events { get; init; } = [];
}

public sealed class EdgeBusinessEventRequest
{
    public string EventId { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public int SchemaVersion { get; init; } = 1;
    public DateTimeOffset OccurredAtUtc { get; init; }
    public Guid? OrderId { get; init; }
    public string EpcHex { get; init; } = string.Empty;
    public string? MovementType { get; init; }
    public string? FromZoneId { get; init; }
    public string? ToZoneId { get; init; }
    public string? ZoneId { get; init; }
    public string ReaderId { get; init; } = string.Empty;
    public int? AntennaId { get; init; }
    public decimal? Rssi { get; init; }
    public string? Reason { get; init; }
    public Dictionary<string, object?> Metadata { get; init; } = new(StringComparer.Ordinal);
}

public sealed record EdgeBatchResultItem(string EventId, string Status, string? Reason);

public sealed record EdgeEventsBatchResponse(int Accepted, int Rejected, IReadOnlyList<EdgeBatchResultItem> Results);

public sealed record EdgeOrderItemResponse(string EpcHex);

public sealed record EdgeActiveOrderResponse(
    string OrderId,
    string Status,
    DateTimeOffset ValidFromUtc,
    DateTimeOffset ValidToUtc,
    IReadOnlyList<string> AllowedZones,
    string MovementPolicy,
    IReadOnlyList<EdgeOrderItemResponse> Items);

public sealed record EdgeActiveOrdersResponse(
    string Version,
    DateTimeOffset GeneratedAtUtc,
    IReadOnlyList<EdgeActiveOrderResponse> Orders);

public sealed record EdgeReadyOrderResponse(
    string Id,
    string OrderNumber,
    string OrderType,
    IReadOnlyList<string> Epcs);

public sealed record EdgeReadyOrdersResponse(
    string Version,
    DateTimeOffset GeneratedAtUtc,
    IReadOnlyList<EdgeReadyOrderResponse> Orders);
