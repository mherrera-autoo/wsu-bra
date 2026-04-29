using ERP.Modules.Rfid.Application.Repositories;
using ERP.Modules.Rfid.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class RfidTagRepository : IRfidTagRepository
{
    private readonly ErpDbContext _dbContext;
    public RfidTagRepository(ErpDbContext dbContext) => _dbContext = dbContext;

    public Task<RfidTag?> GetByPublicIdAsync(Guid companyPublicId, Guid publicId, CancellationToken cancellationToken) =>
        _dbContext.RfidTags.FirstOrDefaultAsync(x => x.CompanyPublicId == companyPublicId && x.PublicId == publicId, cancellationToken);

    public Task<RfidTag?> GetByEpcAsync(Guid companyPublicId, Guid? warehousePublicId, string epc, CancellationToken cancellationToken)
    {
        var query = _dbContext.RfidTags.Where(x => x.CompanyPublicId == companyPublicId && x.Epc == epc);

        if (warehousePublicId.HasValue)
        {
            query = query.Where(x => x.WarehousePublicId == warehousePublicId.Value);
        }

        return query
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RfidTag>> ListAsync(Guid companyPublicId, RfidTagStatus? status, Guid? warehousePublicId, string? epcPrefix, int skip, int take, CancellationToken cancellationToken)
    {
        var q = _dbContext.RfidTags.Where(x => x.CompanyPublicId == companyPublicId);
        if (status.HasValue) q = q.Where(x => x.Status == status);
        if (warehousePublicId.HasValue) q = q.Where(x => x.WarehousePublicId == warehousePublicId.Value);
        if (!string.IsNullOrWhiteSpace(epcPrefix)) q = q.Where(x => x.Epc.StartsWith(epcPrefix));
        return await q.OrderByDescending(x => x.CreatedAt).Skip(skip).Take(take).ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(Guid companyPublicId, RfidTagStatus? status, Guid? warehousePublicId, string? epcPrefix, CancellationToken cancellationToken)
    {
        var q = _dbContext.RfidTags.Where(x => x.CompanyPublicId == companyPublicId);
        if (status.HasValue) q = q.Where(x => x.Status == status);
        if (warehousePublicId.HasValue) q = q.Where(x => x.WarehousePublicId == warehousePublicId.Value);
        if (!string.IsNullOrWhiteSpace(epcPrefix)) q = q.Where(x => x.Epc.StartsWith(epcPrefix));
        return q.CountAsync(cancellationToken);
    }

    public Task AddAsync(RfidTag tag, CancellationToken cancellationToken) => _dbContext.RfidTags.AddAsync(tag, cancellationToken).AsTask();
}

public sealed class RfidEventInboxRepository : IRfidEventInboxRepository
{
    private readonly ErpDbContext _dbContext;
    public RfidEventInboxRepository(ErpDbContext dbContext) => _dbContext = dbContext;
    public Task<RfidEventInbox?> GetByEventIdAsync(Guid companyPublicId, string eventId, CancellationToken cancellationToken) =>
        _dbContext.RfidEventInboxes.FirstOrDefaultAsync(x => x.CompanyPublicId == companyPublicId && x.EventId == eventId, cancellationToken);
    public Task<RfidEventInbox?> GetByPublicIdAsync(Guid companyPublicId, Guid publicId, CancellationToken cancellationToken) =>
        _dbContext.RfidEventInboxes.FirstOrDefaultAsync(x => x.CompanyPublicId == companyPublicId && x.PublicId == publicId, cancellationToken);
    public async Task<IReadOnlyList<RfidEventInbox>> ListAsync(Guid companyPublicId, RfidEventType? type, RfidInboxProcessingStatus? status, Guid? warehousePublicId, string? deviceCode, DateTimeOffset? occurredFrom, DateTimeOffset? occurredTo, int skip, int take, CancellationToken cancellationToken)
    {
        var q = Filter(companyPublicId, type, status, warehousePublicId, deviceCode, occurredFrom, occurredTo);
        return await q.OrderByDescending(x => x.OccurredAt).Skip(skip).Take(take).ToListAsync(cancellationToken);
    }
    public Task<int> CountAsync(Guid companyPublicId, RfidEventType? type, RfidInboxProcessingStatus? status, Guid? warehousePublicId, string? deviceCode, DateTimeOffset? occurredFrom, DateTimeOffset? occurredTo, CancellationToken cancellationToken)
        => Filter(companyPublicId, type, status, warehousePublicId, deviceCode, occurredFrom, occurredTo).CountAsync(cancellationToken);
    public Task AddAsync(RfidEventInbox inbox, CancellationToken cancellationToken) => _dbContext.RfidEventInboxes.AddAsync(inbox, cancellationToken).AsTask();

    private IQueryable<RfidEventInbox> Filter(Guid companyPublicId, RfidEventType? type, RfidInboxProcessingStatus? status, Guid? warehousePublicId, string? deviceCode, DateTimeOffset? occurredFrom, DateTimeOffset? occurredTo)
    {
        var q = _dbContext.RfidEventInboxes.Where(x => x.CompanyPublicId == companyPublicId);
        if (type.HasValue) q = q.Where(x => x.EventType == type);
        if (status.HasValue) q = q.Where(x => x.ProcessingStatus == status);
        if (warehousePublicId.HasValue) q = q.Where(x => x.WarehousePublicId == warehousePublicId);
        if (!string.IsNullOrWhiteSpace(deviceCode)) q = q.Where(x => x.DeviceCode == deviceCode);
        if (occurredFrom.HasValue) q = q.Where(x => x.OccurredAt >= occurredFrom);
        if (occurredTo.HasValue) q = q.Where(x => x.OccurredAt <= occurredTo);
        return q;
    }
}

public sealed class RfidAccessSessionRepository : IRfidAccessSessionRepository
{
    private readonly ErpDbContext _dbContext;
    public RfidAccessSessionRepository(ErpDbContext dbContext) => _dbContext = dbContext;
    public Task<RfidAccessSession?> GetBySessionIdAsync(Guid companyPublicId, string sessionId, CancellationToken cancellationToken) =>
        _dbContext.RfidAccessSessions.FirstOrDefaultAsync(x => x.CompanyPublicId == companyPublicId && x.SessionId == sessionId, cancellationToken);
    public Task<RfidAccessSession?> GetByPublicIdAsync(Guid companyPublicId, Guid publicId, CancellationToken cancellationToken) =>
        _dbContext.RfidAccessSessions.FirstOrDefaultAsync(x => x.CompanyPublicId == companyPublicId && x.PublicId == publicId, cancellationToken);
    public Task<RfidAccessSession?> GetActiveByWarehouseAsync(Guid companyPublicId, Guid warehousePublicId, CancellationToken cancellationToken) =>
        _dbContext.RfidAccessSessions.FirstOrDefaultAsync(x => x.CompanyPublicId == companyPublicId && x.WarehousePublicId == warehousePublicId && x.Status == RfidAccessSessionStatus.Active, cancellationToken);
    public async Task<IReadOnlyList<RfidAccessSession>> ListAsync(Guid companyPublicId, Guid? userPublicId, Guid? warehousePublicId, RfidAccessSessionStatus? status, DateTimeOffset? startedFrom, DateTimeOffset? startedTo, int skip, int take, CancellationToken cancellationToken)
    {
        var q = Filter(companyPublicId, userPublicId, warehousePublicId, status, startedFrom, startedTo);
        return await q.OrderByDescending(x => x.StartedAt).Skip(skip).Take(take).ToListAsync(cancellationToken);
    }
    public Task<int> CountAsync(Guid companyPublicId, Guid? userPublicId, Guid? warehousePublicId, RfidAccessSessionStatus? status, DateTimeOffset? startedFrom, DateTimeOffset? startedTo, CancellationToken cancellationToken)
        => Filter(companyPublicId, userPublicId, warehousePublicId, status, startedFrom, startedTo).CountAsync(cancellationToken);
    public Task AddAsync(RfidAccessSession session, CancellationToken cancellationToken) => _dbContext.RfidAccessSessions.AddAsync(session, cancellationToken).AsTask();

    public async Task<IReadOnlyList<RfidResolvedAccessSessionListItem>> ListResolvedAsync(Guid companyPublicId, Guid? warehousePublicId, DateTimeOffset? from, DateTimeOffset? to, string? deviceCode, long? operatorId, int? status, int skip, int take, CancellationToken cancellationToken)
    {
        var filtered = FilterResolved(companyPublicId, warehousePublicId, from, to, deviceCode, operatorId, status);
        return await filtered
            .Join(_dbContext.Operators,
                session => session.OperatorId,
                op => op.Id,
                (session, op) => new { session, op })
            .OrderByDescending(item => item.session.StartedAt)
            .Skip(skip)
            .Take(take)
            .Select(item => new RfidResolvedAccessSessionListItem(
                item.session.StartedAt,
                item.session.EndedAt,
                (int)item.session.Status,
                item.session.DeviceCode,
                item.session.SessionId,
                item.session.WarehousePublicId,
                item.session.OperatorId,
                item.op.PublicId,
                item.op.FullName,
                item.session.FaceTemplateId,
                item.session.NfcCardUid))
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountResolvedAsync(Guid companyPublicId, Guid? warehousePublicId, DateTimeOffset? from, DateTimeOffset? to, string? deviceCode, long? operatorId, int? status, CancellationToken cancellationToken)
    {
        var query = FilterResolved(companyPublicId, warehousePublicId, from, to, deviceCode, operatorId, status)
            .Join(_dbContext.Operators,
                session => session.OperatorId,
                op => op.Id,
                (session, op) => session.Id);

        return query.CountAsync(cancellationToken);
    }

    public Task<bool> OperatorExistsAsync(Guid companyPublicId, long operatorId, CancellationToken cancellationToken)
        => _dbContext.Operators.AnyAsync(item => item.CompanyPublicId == companyPublicId && item.Id == operatorId, cancellationToken);

    private IQueryable<RfidAccessSession> Filter(Guid companyPublicId, Guid? userPublicId, Guid? warehousePublicId, RfidAccessSessionStatus? status, DateTimeOffset? startedFrom, DateTimeOffset? startedTo)
    {
        var q = _dbContext.RfidAccessSessions.Where(x => x.CompanyPublicId == companyPublicId);
        if (userPublicId.HasValue) q = q.Where(x => x.UserPublicId == userPublicId);
        if (warehousePublicId.HasValue) q = q.Where(x => x.WarehousePublicId == warehousePublicId);
        if (status.HasValue) q = q.Where(x => x.Status == status);
        if (startedFrom.HasValue) q = q.Where(x => x.StartedAt >= startedFrom);
        if (startedTo.HasValue) q = q.Where(x => x.StartedAt <= startedTo);
        return q;
    }

    private IQueryable<RfidAccessSession> FilterResolved(Guid companyPublicId, Guid? warehousePublicId, DateTimeOffset? from, DateTimeOffset? to, string? deviceCode, long? operatorId, int? status)
    {
        var query = _dbContext.RfidAccessSessions
            .Where(item => item.CompanyPublicId == companyPublicId && item.OperatorId > 0);

        if (warehousePublicId.HasValue)
        {
            query = query.Where(item => item.WarehousePublicId == warehousePublicId.Value);
        }
        if (from.HasValue)
        {
            query = query.Where(item => item.StartedAt >= from.Value);
        }
        if (to.HasValue)
        {
            query = query.Where(item => item.StartedAt <= to.Value);
        }
        if (!string.IsNullOrWhiteSpace(deviceCode))
        {
            query = query.Where(item => item.DeviceCode == deviceCode);
        }
        if (operatorId.HasValue)
        {
            query = query.Where(item => item.OperatorId == operatorId.Value);
        }
        if (status.HasValue)
        {
            query = query.Where(item => (int)item.Status == status.Value);
        }

        return query;
    }
}

public sealed class RfidOperatorRepository : IRfidOperatorRepository
{
    private readonly ErpDbContext _dbContext;

    public RfidOperatorRepository(ErpDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<Operator>> ListAsync(Guid companyPublicId, string? search, bool? isActive, int skip, int take, CancellationToken cancellationToken)
    {
        return await ApplyFilter(companyPublicId, search, isActive)
            .OrderBy(item => item.FullName)
            .ThenBy(item => item.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(Guid companyPublicId, string? search, bool? isActive, CancellationToken cancellationToken)
        => ApplyFilter(companyPublicId, search, isActive).CountAsync(cancellationToken);

    public async Task<IReadOnlyList<Operator>> GetByPublicIdsAsync(
        Guid companyPublicId,
        IReadOnlyCollection<Guid> operatorPublicIds,
        CancellationToken cancellationToken)
    {
        if (operatorPublicIds.Count == 0)
        {
            return Array.Empty<Operator>();
        }

        return await _dbContext.Operators
            .Where(item => item.CompanyPublicId == companyPublicId && operatorPublicIds.Contains(item.PublicId))
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Operator> ApplyFilter(Guid companyPublicId, string? search, bool? isActive)
    {
        var query = _dbContext.Operators.Where(item => item.CompanyPublicId == companyPublicId);
        if (isActive.HasValue)
        {
            query = query.Where(item => item.IsActive == isActive.Value);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim();
            query = query.Where(item => item.FullName.Contains(q) || (item.DocumentId != null && item.DocumentId.Contains(q)));
        }

        return query;
    }
}

public sealed class RfidOperatorCredentialRepository : IRfidOperatorCredentialRepository
{
    private readonly ErpDbContext _dbContext;

    public RfidOperatorCredentialRepository(ErpDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<OperatorCredential>> ListActiveByOperatorAsync(Guid companyPublicId, long operatorId, CancellationToken cancellationToken)
    {
        return await _dbContext.OperatorCredentials
            .Where(item => item.CompanyPublicId == companyPublicId && item.OperatorId == operatorId && item.IsActive)
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

public sealed class RfidMovementLinkRepository : IRfidMovementLinkRepository
{
    private readonly ErpDbContext _dbContext;
    public RfidMovementLinkRepository(ErpDbContext dbContext) => _dbContext = dbContext;
    public Task AddAsync(RfidMovementLink link, CancellationToken cancellationToken) => _dbContext.RfidMovementLinks.AddAsync(link, cancellationToken).AsTask();
    public Task<RfidMovementLink?> GetByPublicIdAsync(Guid companyPublicId, Guid publicId, CancellationToken cancellationToken) =>
        _dbContext.RfidMovementLinks.FirstOrDefaultAsync(x => x.CompanyPublicId == companyPublicId && x.PublicId == publicId, cancellationToken);

    public async Task<IReadOnlyList<RfidMovementLink>> ListAsync(Guid companyPublicId, string? eventId, string? sessionId, string? epc, Guid? productPublicId, Guid? movementPublicId, DateTimeOffset? createdFrom, DateTimeOffset? createdTo, int skip, int take, CancellationToken cancellationToken)
    {
        var q = Filter(companyPublicId, eventId, sessionId, epc, productPublicId, movementPublicId, createdFrom, createdTo);
        return await q.OrderByDescending(x => x.CreatedAt).Skip(skip).Take(take).ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(Guid companyPublicId, string? eventId, string? sessionId, string? epc, Guid? productPublicId, Guid? movementPublicId, DateTimeOffset? createdFrom, DateTimeOffset? createdTo, CancellationToken cancellationToken)
        => Filter(companyPublicId, eventId, sessionId, epc, productPublicId, movementPublicId, createdFrom, createdTo).CountAsync(cancellationToken);

    public void Remove(RfidMovementLink link) => _dbContext.RfidMovementLinks.Remove(link);

    private IQueryable<RfidMovementLink> Filter(Guid companyPublicId, string? eventId, string? sessionId, string? epc, Guid? productPublicId, Guid? movementPublicId, DateTimeOffset? createdFrom, DateTimeOffset? createdTo)
    {
        var q = _dbContext.RfidMovementLinks.Where(x => x.CompanyPublicId == companyPublicId);
        if (!string.IsNullOrWhiteSpace(eventId)) q = q.Where(x => x.EventId == eventId);
        if (!string.IsNullOrWhiteSpace(sessionId)) q = q.Where(x => x.SessionId == sessionId);
        if (!string.IsNullOrWhiteSpace(epc)) q = q.Where(x => x.Epc == epc);
        if (productPublicId.HasValue) q = q.Where(x => x.ProductPublicId == productPublicId);
        if (movementPublicId.HasValue) q = q.Where(x => x.InventoryMovementPublicId == movementPublicId);
        if (createdFrom.HasValue) q = q.Where(x => x.CreatedAt >= createdFrom.Value.UtcDateTime);
        if (createdTo.HasValue) q = q.Where(x => x.CreatedAt <= createdTo.Value.UtcDateTime);
        return q;
    }
}

public sealed class RfidReferenceResolver : IRfidReferenceResolver
{
    private readonly ErpDbContext _dbContext;

    public RfidReferenceResolver(ErpDbContext dbContext) => _dbContext = dbContext;

    public Task<long?> ResolveProductIdAsync(Guid companyPublicId, Guid productPublicId, CancellationToken cancellationToken)
        => (from product in _dbContext.Products
            join company in _dbContext.Companies on product.CompanyId equals company.Id
            where company.PublicId == companyPublicId && product.PublicId == productPublicId
            select (long?)product.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<long?> ResolveWarehouseIdAsync(Guid companyPublicId, Guid warehousePublicId, CancellationToken cancellationToken)
        => (from warehouse in _dbContext.Warehouses
            join company in _dbContext.Companies on warehouse.CompanyId equals company.Id
            where company.PublicId == companyPublicId && warehouse.PublicId == warehousePublicId
            select (long?)warehouse.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<Guid?> ResolveProductPublicIdByEpcAsync(Guid companyPublicId, Guid warehousePublicId, string epc, CancellationToken cancellationToken)
        => (from assignment in _dbContext.WsuOrderItemEpcAssignments
            join order in _dbContext.WsuOrders on assignment.OrderId equals order.Id
            join item in _dbContext.WsuOrderItems on assignment.OrderItemId equals item.Id
            where order.CompanyPublicId == companyPublicId
                && order.WarehousePublicId == warehousePublicId
                && assignment.Epc == epc
                && item.ProductPublicId != null
            orderby assignment.Id descending
            select item.ProductPublicId)
            .FirstOrDefaultAsync(cancellationToken);
}
