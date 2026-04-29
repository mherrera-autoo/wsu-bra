using ERP.Modules.Inventory.Application.Repositories;
using ERP.Modules.Inventory.Domain;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.Rfid.Application.Commands;
using ERP.Modules.Rfid.Application.Repositories;
using ERP.Modules.Rfid.Application.Services;
using ERP.Modules.Rfid.Domain;
using Xunit;

namespace ERP.Modules.Rfid.WhiteBox.Tests;

public sealed class RfidServiceTests
{
    private static readonly Guid CompanyPublicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ProductPublicId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid WarehousePublicId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid UserPublicId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    [Fact]
    public async Task IngestEvent_IsIdempotent_ByEventId()
    {
        var fixture = new Fixture();
        await fixture.Service.StartAccessSessionAsync(CompanyPublicId, new StartAccessSessionCommand("session-1", UserPublicId, WarehousePublicId, null, DateTimeOffset.Parse("2026-01-01T10:00:00Z"), "EDGE-01"), default);
        await fixture.Service.RegisterTagAsync(CompanyPublicId, new RegisterTagCommand(WarehousePublicId, "EPC-001", null), default);

        var first = await fixture.Service.IngestPortalExitEventAsync(CompanyPublicId, new IngestPortalExitEventCommand("evt-1", DateTimeOffset.Parse("2026-01-01T10:05:00Z"), "PORTAL-01", WarehousePublicId, ["EPC-001"], 0.99m), 900, default);
        var second = await fixture.Service.IngestPortalExitEventAsync(CompanyPublicId, new IngestPortalExitEventCommand("evt-1", DateTimeOffset.Parse("2026-01-01T10:05:00Z"), "PORTAL-01", WarehousePublicId, ["EPC-001"], 0.99m), 900, default);

        Assert.False(first.Duplicated);
        Assert.True(second.Duplicated);
        Assert.Single(fixture.InventoryMovements);
    }

    [Fact]
    public async Task IngestEvent_CreatesOutboundMovement_WithRfidSource_AndSessionReference()
    {
        var fixture = new Fixture();
        await fixture.Service.StartAccessSessionAsync(CompanyPublicId, new StartAccessSessionCommand("session-2", UserPublicId, WarehousePublicId, null, DateTimeOffset.Parse("2026-01-01T10:00:00Z"), "EDGE-01"), default);
        await fixture.Service.RegisterTagAsync(CompanyPublicId, new RegisterTagCommand(WarehousePublicId, "EPC-002", null), default);

        var result = await fixture.Service.IngestPortalExitEventAsync(CompanyPublicId, new IngestPortalExitEventCommand("evt-2", DateTimeOffset.Parse("2026-01-01T10:05:00Z"), "PORTAL-01", WarehousePublicId, ["EPC-002"], 0.91m), 900, default);

        Assert.Equal(1, result.MovementsCreated);
        var movement = Assert.Single(fixture.InventoryMovements);
        Assert.Equal("RFID", movement.Source);
        Assert.Equal("RFID_SESSION", movement.ReferenceType);
        Assert.Equal("session-2", movement.ReferenceId);
    }

    [Fact]
    public async Task IngestEvent_KeepsUnmappedEpcs_WithoutFailingWholeBatch()
    {
        var fixture = new Fixture();
        await fixture.Service.StartAccessSessionAsync(CompanyPublicId, new StartAccessSessionCommand("session-3", UserPublicId, WarehousePublicId, null, DateTimeOffset.Parse("2026-01-01T10:00:00Z"), "EDGE-01"), default);
        await fixture.Service.RegisterTagAsync(CompanyPublicId, new RegisterTagCommand(WarehousePublicId, "EPC-003", null), default);

        var result = await fixture.Service.IngestPortalExitEventAsync(CompanyPublicId, new IngestPortalExitEventCommand("evt-3", DateTimeOffset.Parse("2026-01-01T10:05:00Z"), "PORTAL-01", WarehousePublicId, ["EPC-003", "EPC-404"], null), 900, default);

        Assert.Equal(1, result.MovementsCreated);
        Assert.Single(result.UnmappedEpcs);
        Assert.Equal("EPC-404", result.UnmappedEpcs.Single());
    }

    [Fact]
    public async Task StartEndSession_AreIdempotent()
    {
        var fixture = new Fixture();
        var first = await fixture.Service.StartAccessSessionAsync(CompanyPublicId, new StartAccessSessionCommand("session-4", UserPublicId, WarehousePublicId, null, DateTimeOffset.Parse("2026-01-01T10:00:00Z"), "EDGE-01"), default);
        var second = await fixture.Service.StartAccessSessionAsync(CompanyPublicId, new StartAccessSessionCommand("session-4", UserPublicId, WarehousePublicId, null, DateTimeOffset.Parse("2026-01-01T10:01:00Z"), "EDGE-01"), default);
        await fixture.Service.EndAccessSessionAsync(CompanyPublicId, new EndAccessSessionCommand("session-4", DateTimeOffset.Parse("2026-01-01T10:30:00Z")), default);
        await fixture.Service.EndAccessSessionAsync(CompanyPublicId, new EndAccessSessionCommand("session-4", DateTimeOffset.Parse("2026-01-01T10:40:00Z")), default);

        Assert.Equal(first.PublicId, second.PublicId);
        Assert.Equal(RfidAccessSessionStatus.Closed, first.Status);
        Assert.Equal(DateTimeOffset.Parse("2026-01-01T10:30:00Z"), first.EndedAt);
    }

    private sealed class Fixture
    {
        public List<InventoryMovement> InventoryMovements { get; } = [];
        public RfidService Service { get; }

        public Fixture()
        {
            var tags = new List<RfidTag>();
            var sessions = new List<RfidAccessSession>();
            var inbox = new List<RfidEventInbox>();
            var links = new List<RfidMovementLink>();
            Service = new RfidService(new TagRepo(tags), new SessionRepo(sessions), new InboxRepo(inbox), new LinkRepo(links), new ReferenceResolver(), new InventoryRepo(InventoryMovements), new CompanyRepo());
        }
    }

    private sealed class ReferenceResolver : IRfidReferenceResolver
    {
        public Task<long?> ResolveProductIdAsync(Guid companyPublicId, Guid productPublicId, CancellationToken cancellationToken)
            => Task.FromResult<long?>(999);

        public Task<long?> ResolveWarehouseIdAsync(Guid companyPublicId, Guid warehousePublicId, CancellationToken cancellationToken)
            => Task.FromResult<long?>(50);

        public Task<Guid?> ResolveProductPublicIdByEpcAsync(Guid companyPublicId, Guid warehousePublicId, string epc, CancellationToken cancellationToken)
            => Task.FromResult<Guid?>(ProductPublicId);
    }

    private sealed class CompanyRepo : ICompanyRepository
    {
        public Task<IReadOnlyList<CompanySnapshot>> ListAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<CompanySnapshot>>([]);
        public Task<CompanySnapshot?> GetByIdAsync(long id, CancellationToken cancellationToken = default) => Task.FromResult<CompanySnapshot?>(new CompanySnapshot(id, CompanyPublicId, 1, 1, "Test"));
        public Task<CompanySnapshot?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default) => Task.FromResult<CompanySnapshot?>(new CompanySnapshot(1, publicId, 1, 1, "Test"));
        public Task<long?> GetIdByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default) => Task.FromResult<long?>(1);
        public Task<CompanySnapshot?> GetByTaxEntityIdAsync(long taxEntityId, CancellationToken cancellationToken = default) => Task.FromResult<CompanySnapshot?>(null);
        public Task<long?> GetOrganizationIdAsync(long companyId, CancellationToken cancellationToken = default) => Task.FromResult<long?>(1);
        public Task<CompanySnapshot> AddAsync(CompanyCreateRequest company, CancellationToken cancellationToken = default) => Task.FromResult(new CompanySnapshot(1, CompanyPublicId, company.OrganizationId, company.TaxEntityId, company.Name));
    }

    private sealed class InventoryRepo(List<InventoryMovement> items) : IInventoryMovementRepository
    {
        public Task AddAsync(InventoryMovement movement, CancellationToken cancellationToken = default) { items.Add(movement); return Task.CompletedTask; }
        public Task<IReadOnlyList<InventoryMovement>> ListByReferenceAsync(long companyId, string referenceType, string referenceId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<InventoryMovement>>([]);
        public Task<bool> ExistsBySourceAsync(long companyId, string source, string sourceId, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }
    private sealed class TagRepo(List<RfidTag> items) : IRfidTagRepository
    {
        public Task AddAsync(RfidTag tag, CancellationToken cancellationToken) { items.Add(tag); return Task.CompletedTask; }
        public Task<int> CountAsync(Guid companyPublicId, RfidTagStatus? status, Guid? warehousePublicId, string? epcPrefix, CancellationToken cancellationToken) => Task.FromResult(0);
        public Task<RfidTag?> GetByEpcAsync(Guid companyPublicId, Guid? warehousePublicId, string epc, CancellationToken cancellationToken)
            => Task.FromResult(items.FirstOrDefault(x => x.CompanyPublicId == companyPublicId && x.WarehousePublicId == warehousePublicId && x.Epc == epc));
        public Task<RfidTag?> GetByPublicIdAsync(Guid companyPublicId, Guid publicId, CancellationToken cancellationToken) => Task.FromResult(items.FirstOrDefault(x => x.CompanyPublicId == companyPublicId && x.PublicId == publicId));
        public Task<IReadOnlyList<RfidTag>> ListAsync(Guid companyPublicId, RfidTagStatus? status, Guid? warehousePublicId, string? epcPrefix, int skip, int take, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<RfidTag>>([]);
    }
    private sealed class SessionRepo(List<RfidAccessSession> items) : IRfidAccessSessionRepository
    {
        public Task AddAsync(RfidAccessSession session, CancellationToken cancellationToken) { items.Add(session); return Task.CompletedTask; }
        public Task<int> CountAsync(Guid companyPublicId, Guid? userPublicId, Guid? warehousePublicId, RfidAccessSessionStatus? status, DateTimeOffset? startedFrom, DateTimeOffset? startedTo, CancellationToken cancellationToken) => Task.FromResult(0);
        public Task<int> CountResolvedAsync(Guid companyPublicId, Guid? warehousePublicId, DateTimeOffset? from, DateTimeOffset? to, string? deviceCode, long? operatorId, int? status, CancellationToken cancellationToken) => Task.FromResult(0);
        public Task<RfidAccessSession?> GetActiveByWarehouseAsync(Guid companyPublicId, Guid warehousePublicId, CancellationToken cancellationToken) => Task.FromResult(items.FirstOrDefault(x => x.CompanyPublicId == companyPublicId && x.WarehousePublicId == warehousePublicId && x.Status == RfidAccessSessionStatus.Active));
        public Task<RfidAccessSession?> GetByPublicIdAsync(Guid companyPublicId, Guid publicId, CancellationToken cancellationToken) => Task.FromResult(items.FirstOrDefault(x => x.CompanyPublicId == companyPublicId && x.PublicId == publicId));
        public Task<RfidAccessSession?> GetBySessionIdAsync(Guid companyPublicId, string sessionId, CancellationToken cancellationToken) => Task.FromResult(items.FirstOrDefault(x => x.CompanyPublicId == companyPublicId && x.SessionId == sessionId));
        public Task<IReadOnlyList<RfidAccessSession>> ListAsync(Guid companyPublicId, Guid? userPublicId, Guid? warehousePublicId, RfidAccessSessionStatus? status, DateTimeOffset? startedFrom, DateTimeOffset? startedTo, int skip, int take, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<RfidAccessSession>>([]);
        public Task<IReadOnlyList<RfidResolvedAccessSessionListItem>> ListResolvedAsync(Guid companyPublicId, Guid? warehousePublicId, DateTimeOffset? from, DateTimeOffset? to, string? deviceCode, long? operatorId, int? status, int skip, int take, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<RfidResolvedAccessSessionListItem>>([]);
        public Task<bool> OperatorExistsAsync(Guid companyPublicId, long operatorId, CancellationToken cancellationToken) => Task.FromResult(false);
    }
    private sealed class InboxRepo(List<RfidEventInbox> items) : IRfidEventInboxRepository
    {
        public Task AddAsync(RfidEventInbox inbox, CancellationToken cancellationToken) { items.Add(inbox); return Task.CompletedTask; }
        public Task<int> CountAsync(Guid companyPublicId, RfidEventType? type, RfidInboxProcessingStatus? status, Guid? warehousePublicId, string? deviceCode, DateTimeOffset? occurredFrom, DateTimeOffset? occurredTo, CancellationToken cancellationToken) => Task.FromResult(0);
        public Task<RfidEventInbox?> GetByEventIdAsync(Guid companyPublicId, string eventId, CancellationToken cancellationToken) => Task.FromResult(items.FirstOrDefault(x => x.CompanyPublicId == companyPublicId && x.EventId == eventId));
        public Task<RfidEventInbox?> GetByPublicIdAsync(Guid companyPublicId, Guid publicId, CancellationToken cancellationToken) => Task.FromResult(items.FirstOrDefault(x => x.CompanyPublicId == companyPublicId && x.PublicId == publicId));
        public Task<IReadOnlyList<RfidEventInbox>> ListAsync(Guid companyPublicId, RfidEventType? type, RfidInboxProcessingStatus? status, Guid? warehousePublicId, string? deviceCode, DateTimeOffset? occurredFrom, DateTimeOffset? occurredTo, int skip, int take, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<RfidEventInbox>>([]);
    }
    private sealed class LinkRepo(List<RfidMovementLink> items) : IRfidMovementLinkRepository
    {
        public Task AddAsync(RfidMovementLink link, CancellationToken cancellationToken) { items.Add(link); return Task.CompletedTask; }
        public Task<int> CountAsync(Guid companyPublicId, string? eventId, string? sessionId, string? epc, Guid? productPublicId, Guid? movementPublicId, DateTimeOffset? createdFrom, DateTimeOffset? createdTo, CancellationToken cancellationToken) => Task.FromResult(0);
        public Task<RfidMovementLink?> GetByPublicIdAsync(Guid companyPublicId, Guid publicId, CancellationToken cancellationToken) => Task.FromResult(items.FirstOrDefault(x => x.CompanyPublicId == companyPublicId && x.PublicId == publicId));
        public Task<IReadOnlyList<RfidMovementLink>> ListAsync(Guid companyPublicId, string? eventId, string? sessionId, string? epc, Guid? productPublicId, Guid? movementPublicId, DateTimeOffset? createdFrom, DateTimeOffset? createdTo, int skip, int take, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<RfidMovementLink>>([]);
        public void Remove(RfidMovementLink link) { }
    }
}
