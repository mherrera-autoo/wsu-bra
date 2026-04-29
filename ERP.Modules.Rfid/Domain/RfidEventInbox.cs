namespace ERP.Modules.Rfid.Domain;

public sealed class RfidEventInbox : ERP.Shared.Domain.Entity
{
    public Guid CompanyPublicId { get; private set; }
    public string EventId { get; private set; } = null!;
    public RfidEventType EventType { get; private set; }
    public string DeviceCode { get; private set; } = null!;
    public Guid WarehousePublicId { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public decimal? Confidence { get; private set; }
    public string PayloadJson { get; private set; } = null!;
    public RfidInboxProcessingStatus ProcessingStatus { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public string? Error { get; private set; }

    private RfidEventInbox() { }

    public static RfidEventInbox Create(Guid companyPublicId, string eventId, RfidEventType eventType, string deviceCode, Guid warehousePublicId, DateTimeOffset occurredAt, decimal? confidence, string payloadJson)
    {
        return new RfidEventInbox
        {
            CompanyPublicId = companyPublicId,
            EventId = eventId.Trim(),
            EventType = eventType,
            DeviceCode = deviceCode.Trim(),
            WarehousePublicId = warehousePublicId,
            OccurredAt = occurredAt,
            Confidence = confidence,
            PayloadJson = payloadJson,
            ProcessingStatus = RfidInboxProcessingStatus.Pending
        };
    }

    public void MarkProcessed(string? warning = null)
    {
        ProcessingStatus = RfidInboxProcessingStatus.Processed;
        ProcessedAt = DateTimeOffset.UtcNow;
        Error = warning;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string error)
    {
        ProcessingStatus = RfidInboxProcessingStatus.Failed;
        ProcessedAt = DateTimeOffset.UtcNow;
        Error = error;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAdmin(RfidInboxProcessingStatus status, string? error, string? payloadJson)
    {
        ProcessingStatus = status;
        Error = error;
        if (!string.IsNullOrWhiteSpace(payloadJson))
        {
            PayloadJson = payloadJson;
        }
        UpdatedAt = DateTime.UtcNow;
    }
}
