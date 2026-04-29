namespace ERP.Modules.Rfid.Domain;

public sealed class RfidAccessSession : ERP.Shared.Domain.Entity
{
    public Guid CompanyPublicId { get; private set; }
    public string? SessionId { get; private set; }
    public Guid UserPublicId { get; private set; }
    public Guid WarehousePublicId { get; private set; }
    public string? AccessCardId { get; private set; }
    public string? FaceTemplateId { get; private set; }
    public string? NfcCardUid { get; private set; }
    public string DeviceCode { get; private set; } = null!;
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }
    public RfidAccessSessionStatus Status { get; private set; }
    public long OperatorId { get; private set; }

    public Operator? Operator { get; private set; }

    private RfidAccessSession() { }

    public static RfidAccessSession Create(Guid companyPublicId, string sessionId, Guid userPublicId, Guid warehousePublicId, string? accessCardId, string deviceCode, DateTimeOffset startedAt)
    {
        return new RfidAccessSession
        {
            CompanyPublicId = companyPublicId,
            SessionId = sessionId.Trim(),
            UserPublicId = userPublicId,
            WarehousePublicId = warehousePublicId,
            AccessCardId = accessCardId,
            DeviceCode = deviceCode.Trim(),
            StartedAt = startedAt,
            Status = RfidAccessSessionStatus.Active,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static RfidAccessSession CreateResolved(
        Guid companyPublicId,
        Guid warehousePublicId,
        string deviceCode,
        DateTimeOffset startedAt,
        int status,
        long operatorId,
        string? sessionId,
        DateTimeOffset? endedAt,
        string? faceTemplateId,
        string? nfcCardUid)
    {
        return new RfidAccessSession
        {
            CompanyPublicId = companyPublicId,
            WarehousePublicId = warehousePublicId,
            DeviceCode = deviceCode.Trim(),
            StartedAt = startedAt,
            Status = (RfidAccessSessionStatus)status,
            OperatorId = operatorId,
            SessionId = string.IsNullOrWhiteSpace(sessionId) ? null : sessionId.Trim(),
            EndedAt = endedAt,
            FaceTemplateId = string.IsNullOrWhiteSpace(faceTemplateId) ? null : faceTemplateId.Trim(),
            NfcCardUid = string.IsNullOrWhiteSpace(nfcCardUid) ? null : nfcCardUid.Trim(),
            UserPublicId = Guid.Empty,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Close(DateTimeOffset endedAt)
    {
        if (Status != RfidAccessSessionStatus.Active)
        {
            return;
        }

        EndedAt = endedAt;
        Status = RfidAccessSessionStatus.Closed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMetadata(string? accessCardId, string deviceCode)
    {
        AccessCardId = accessCardId;
        DeviceCode = deviceCode;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAbandoned()
    {
        Status = RfidAccessSessionStatus.Abandoned;
        UpdatedAt = DateTime.UtcNow;
    }
}
