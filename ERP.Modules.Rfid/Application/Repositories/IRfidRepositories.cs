using ERP.Modules.Rfid.Domain;

namespace ERP.Modules.Rfid.Application.Repositories;

public sealed record RfidResolvedAccessSessionListItem(
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

public interface IRfidTagRepository
{
    Task<RfidTag?> GetByPublicIdAsync(Guid companyPublicId, Guid publicId, CancellationToken cancellationToken);
    Task<RfidTag?> GetByEpcAsync(Guid companyPublicId, Guid? warehousePublicId, string epc, CancellationToken cancellationToken);
    Task<IReadOnlyList<RfidTag>> ListAsync(Guid companyPublicId, RfidTagStatus? status, Guid? warehousePublicId, string? epcPrefix, int skip, int take, CancellationToken cancellationToken);
    Task<int> CountAsync(Guid companyPublicId, RfidTagStatus? status, Guid? warehousePublicId, string? epcPrefix, CancellationToken cancellationToken);
    Task AddAsync(RfidTag tag, CancellationToken cancellationToken);
}

public interface IRfidEventInboxRepository
{
    Task<RfidEventInbox?> GetByEventIdAsync(Guid companyPublicId, string eventId, CancellationToken cancellationToken);
    Task<RfidEventInbox?> GetByPublicIdAsync(Guid companyPublicId, Guid publicId, CancellationToken cancellationToken);
    Task<IReadOnlyList<RfidEventInbox>> ListAsync(Guid companyPublicId, RfidEventType? type, RfidInboxProcessingStatus? status, Guid? warehousePublicId, string? deviceCode, DateTimeOffset? occurredFrom, DateTimeOffset? occurredTo, int skip, int take, CancellationToken cancellationToken);
    Task<int> CountAsync(Guid companyPublicId, RfidEventType? type, RfidInboxProcessingStatus? status, Guid? warehousePublicId, string? deviceCode, DateTimeOffset? occurredFrom, DateTimeOffset? occurredTo, CancellationToken cancellationToken);
    Task AddAsync(RfidEventInbox inbox, CancellationToken cancellationToken);
}

public interface IRfidAccessSessionRepository
{
    Task<RfidAccessSession?> GetBySessionIdAsync(Guid companyPublicId, string sessionId, CancellationToken cancellationToken);
    Task<RfidAccessSession?> GetByPublicIdAsync(Guid companyPublicId, Guid publicId, CancellationToken cancellationToken);
    Task<RfidAccessSession?> GetActiveByWarehouseAsync(Guid companyPublicId, Guid warehousePublicId, CancellationToken cancellationToken);
    Task<IReadOnlyList<RfidAccessSession>> ListAsync(Guid companyPublicId, Guid? userPublicId, Guid? warehousePublicId, RfidAccessSessionStatus? status, DateTimeOffset? startedFrom, DateTimeOffset? startedTo, int skip, int take, CancellationToken cancellationToken);
    Task<int> CountAsync(Guid companyPublicId, Guid? userPublicId, Guid? warehousePublicId, RfidAccessSessionStatus? status, DateTimeOffset? startedFrom, DateTimeOffset? startedTo, CancellationToken cancellationToken);
    Task AddAsync(RfidAccessSession session, CancellationToken cancellationToken);
    Task<IReadOnlyList<RfidResolvedAccessSessionListItem>> ListResolvedAsync(Guid companyPublicId, Guid? warehousePublicId, DateTimeOffset? from, DateTimeOffset? to, string? deviceCode, long? operatorId, int? status, int skip, int take, CancellationToken cancellationToken);
    Task<int> CountResolvedAsync(Guid companyPublicId, Guid? warehousePublicId, DateTimeOffset? from, DateTimeOffset? to, string? deviceCode, long? operatorId, int? status, CancellationToken cancellationToken);
    Task<bool> OperatorExistsAsync(Guid companyPublicId, long operatorId, CancellationToken cancellationToken);
}

public interface IRfidOperatorRepository
{
    Task<IReadOnlyList<Operator>> ListAsync(Guid companyPublicId, string? search, bool? isActive, int skip, int take, CancellationToken cancellationToken);
    Task<int> CountAsync(Guid companyPublicId, string? search, bool? isActive, CancellationToken cancellationToken);
    Task<IReadOnlyList<Operator>> GetByPublicIdsAsync(Guid companyPublicId, IReadOnlyCollection<Guid> operatorPublicIds, CancellationToken cancellationToken);
}

public interface IRfidOperatorCredentialRepository
{
    Task<IReadOnlyList<OperatorCredential>> ListActiveByOperatorAsync(Guid companyPublicId, long operatorId, CancellationToken cancellationToken);
}

public interface IRfidMovementLinkRepository
{
    Task AddAsync(RfidMovementLink link, CancellationToken cancellationToken);
    Task<RfidMovementLink?> GetByPublicIdAsync(Guid companyPublicId, Guid publicId, CancellationToken cancellationToken);
    Task<IReadOnlyList<RfidMovementLink>> ListAsync(Guid companyPublicId, string? eventId, string? sessionId, string? epc, Guid? productPublicId, Guid? movementPublicId, DateTimeOffset? createdFrom, DateTimeOffset? createdTo, int skip, int take, CancellationToken cancellationToken);
    Task<int> CountAsync(Guid companyPublicId, string? eventId, string? sessionId, string? epc, Guid? productPublicId, Guid? movementPublicId, DateTimeOffset? createdFrom, DateTimeOffset? createdTo, CancellationToken cancellationToken);
    void Remove(RfidMovementLink link);
}

public interface IRfidReferenceResolver
{
    Task<long?> ResolveProductIdAsync(Guid companyPublicId, Guid productPublicId, CancellationToken cancellationToken);
    Task<long?> ResolveWarehouseIdAsync(Guid companyPublicId, Guid warehousePublicId, CancellationToken cancellationToken);
    Task<Guid?> ResolveProductPublicIdByEpcAsync(Guid companyPublicId, Guid warehousePublicId, string epc, CancellationToken cancellationToken);
}
