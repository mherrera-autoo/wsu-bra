namespace ERP.Modules.Rfid.Domain;

public sealed class EdgeBusinessEvent : ERP.Shared.Domain.Entity
{
    public Guid CompanyPublicId { get; private set; }
    public string EdgeNodeId { get; private set; } = null!;
    public string EventId { get; private set; } = null!;
    public string EventType { get; private set; } = null!;
    public int SchemaVersion { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public Guid? OrderId { get; private set; }
    public string EpcHex { get; private set; } = null!;
    public string? MovementType { get; private set; }
    public string? FromZoneId { get; private set; }
    public string? ToZoneId { get; private set; }
    public string? ZoneId { get; private set; }
    public string ReaderId { get; private set; } = null!;
    public int? AntennaId { get; private set; }
    public decimal? Rssi { get; private set; }
    public string? Reason { get; private set; }
    public string MetadataJson { get; private set; } = "{}";
    public DateTimeOffset ReceivedAtUtc { get; private set; }

    private EdgeBusinessEvent() { }

    public static EdgeBusinessEvent Create(
        Guid companyPublicId,
        string edgeNodeId,
        string eventId,
        string eventType,
        int schemaVersion,
        DateTimeOffset occurredAtUtc,
        Guid? orderId,
        string epcHex,
        string? movementType,
        string? fromZoneId,
        string? toZoneId,
        string? zoneId,
        string readerId,
        int? antennaId,
        decimal? rssi,
        string? reason,
        string metadataJson)
    {
        if (companyPublicId == Guid.Empty) throw new ArgumentException("CompanyPublicId is required.", nameof(companyPublicId));
        if (string.IsNullOrWhiteSpace(edgeNodeId)) throw new ArgumentException("EdgeNodeId is required.", nameof(edgeNodeId));
        if (string.IsNullOrWhiteSpace(eventId)) throw new ArgumentException("EventId is required.", nameof(eventId));
        if (string.IsNullOrWhiteSpace(eventType)) throw new ArgumentException("EventType is required.", nameof(eventType));
        if (string.IsNullOrWhiteSpace(epcHex)) throw new ArgumentException("EpcHex is required.", nameof(epcHex));
        if (string.IsNullOrWhiteSpace(readerId)) throw new ArgumentException("ReaderId is required.", nameof(readerId));

        return new EdgeBusinessEvent
        {
            CompanyPublicId = companyPublicId,
            EdgeNodeId = edgeNodeId.Trim(),
            EventId = eventId.Trim(),
            EventType = eventType.Trim(),
            SchemaVersion = schemaVersion,
            OccurredAtUtc = occurredAtUtc,
            OrderId = orderId,
            EpcHex = epcHex.Trim().ToUpperInvariant(),
            MovementType = NormalizeNullable(movementType),
            FromZoneId = NormalizeNullable(fromZoneId),
            ToZoneId = NormalizeNullable(toZoneId),
            ZoneId = NormalizeNullable(zoneId),
            ReaderId = readerId.Trim(),
            AntennaId = antennaId,
            Rssi = rssi,
            Reason = NormalizeNullable(reason),
            MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? "{}" : metadataJson,
            ReceivedAtUtc = DateTimeOffset.UtcNow
        };
    }

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
