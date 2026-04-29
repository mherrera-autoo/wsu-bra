using ERP.Modules.Rfid.Domain;

namespace ERP.Modules.Rfid.Application.Commands;

public sealed record RegisterTagCommand(Guid? WarehousePublicId, string Epc, string? Notes);
public sealed record StartAccessSessionCommand(string SessionId, Guid UserPublicId, Guid WarehousePublicId, string? AccessCardId, DateTimeOffset StartedAt, string EdgeDeviceId);
public sealed record EndAccessSessionCommand(string SessionId, DateTimeOffset EndedAt);
public sealed record IngestPortalExitEventCommand(string EventId, DateTimeOffset OccurredAt, string DeviceCode, Guid WarehousePublicId, IReadOnlyCollection<string> Epcs, decimal? Confidence);
public sealed record UpdateRfidTagCommand(RfidTagStatus Status, string? Notes);
public sealed record UpdateAccessSessionCommand(string? AccessCardId, string DeviceCode);
public sealed record UpdateRfidEventCommand(RfidInboxProcessingStatus Status, string? Error, string? PayloadJson);
