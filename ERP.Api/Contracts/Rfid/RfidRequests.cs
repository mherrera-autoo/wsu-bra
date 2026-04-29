using ERP.Modules.Rfid.Domain;

namespace ERP.Api.Contracts.Rfid;

public sealed record RfidTagListItemResponse(
    string Epc,
    Guid? WarehousePublicId,
    RfidTagStatus Status,
    string? Notes,
    long Id,
    Guid PublicId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
public sealed record StartAccessSessionRequest(string SessionId, Guid UserPublicId, Guid WarehousePublicId, string? AccessCardId, DateTimeOffset StartedAt, string EdgeDeviceId);
public sealed record EndAccessSessionRequest(DateTimeOffset EndedAt);
public sealed record UpdateAccessSessionRequest(string? AccessCardId, string DeviceCode);
public sealed record CreateResolvedAccessSessionRequest(
    Guid CompanyPublicId,
    Guid WarehousePublicId,
    string DeviceCode,
    DateTimeOffset StartedAt,
    int Status,
    long OperatorId,
    string? SessionId,
    DateTimeOffset? EndedAt,
    string? FaceTemplateId,
    string? NfcCardUid);

public sealed record ListResolvedAccessSessionsQuery(
    Guid CompanyPublicId,
    Guid? WarehousePublicId,
    DateTimeOffset? From,
    DateTimeOffset? To,
    string? DeviceCode,
    long? OperatorId,
    int? Status,
    int Page = 1,
    int PageSize = 50);

public sealed record ListOperatorsQuery(
    Guid? CompanyPublicId,
    string? Search,
    bool? IsActive,
    int Page = 1,
    int PageSize = 50);

public sealed record RfidResolvedAccessSessionResponse(
    DateTimeOffset StartedAt,
    DateTimeOffset? EndedAt,
    int Status,
    string DeviceCode,
    string? SessionId,
    Guid WarehousePublicId,
    long OperatorId,
    Guid OperatorPublicId,
    string OperatorFullName,
    string? FaceTemplateId,
    string? NfcCardUid);

public sealed record RfidOperatorResponse(
    long Id,
    Guid PublicId,
    string FullName,
    string? DocumentId,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record RfidOperatorCredentialResponse(
    long Id,
    Guid PublicId,
    long OperatorId,
    string? FaceTemplateId,
    string? NfcCardUid,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);

public sealed record IngestPortalExitEventRequest(string EventId, DateTimeOffset OccurredAt, string DeviceCode, Guid WarehousePublicId, IReadOnlyCollection<string> Epcs, decimal? Confidence);
public sealed record UpdateRfidEventRequest(RfidInboxProcessingStatus Status, string? Error, string? PayloadJson);
