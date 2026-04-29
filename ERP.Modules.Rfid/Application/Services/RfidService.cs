using System.Text.Json;
using ERP.Modules.Inventory.Application.Repositories;
using ERP.Modules.Inventory.Domain;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.Rfid.Application.Commands;
using ERP.Modules.Rfid.Application.Repositories;
using ERP.Modules.Rfid.Domain;

namespace ERP.Modules.Rfid.Application.Services;

public sealed class RfidService
{
    private const string RfidSource = "RFID";
    private const string ReferenceTypeSession = "RFID_SESSION";
    private const string PortalExitReason = "Auto discount via RFID portal";

    private readonly IRfidTagRepository _tagRepository;
    private readonly IRfidAccessSessionRepository _sessionRepository;
    private readonly IRfidEventInboxRepository _eventInboxRepository;
    private readonly IRfidMovementLinkRepository _movementLinkRepository;
    private readonly IRfidReferenceResolver _referenceResolver;
    private readonly IInventoryMovementRepository _inventoryMovementRepository;
    private readonly ICompanyRepository _companyRepository;

    public RfidService(
        IRfidTagRepository tagRepository,
        IRfidAccessSessionRepository sessionRepository,
        IRfidEventInboxRepository eventInboxRepository,
        IRfidMovementLinkRepository movementLinkRepository,
        IRfidReferenceResolver referenceResolver,
        IInventoryMovementRepository inventoryMovementRepository,
        ICompanyRepository companyRepository)
    {
        _tagRepository = tagRepository;
        _sessionRepository = sessionRepository;
        _eventInboxRepository = eventInboxRepository;
        _movementLinkRepository = movementLinkRepository;
        _referenceResolver = referenceResolver;
        _inventoryMovementRepository = inventoryMovementRepository;
        _companyRepository = companyRepository;
    }

    public async Task<RfidTag> RegisterTagAsync(Guid companyPublicId, RegisterTagCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Epc)) throw new InvalidOperationException("EPC is required.");
        var existing = await _tagRepository.GetByEpcAsync(companyPublicId, command.WarehousePublicId, command.Epc, cancellationToken);
        if (existing is not null)
        {
            existing.Update(RfidTagStatus.Active, command.Notes);
            return existing;
        }

        var tag = RfidTag.Create(companyPublicId, command.WarehousePublicId, command.Epc, command.Notes);
        await _tagRepository.AddAsync(tag, cancellationToken);
        return tag;
    }

    public async Task<RfidAccessSession> StartAccessSessionAsync(Guid companyPublicId, StartAccessSessionCommand command, CancellationToken cancellationToken)
    {
        var existing = await _sessionRepository.GetBySessionIdAsync(companyPublicId, command.SessionId, cancellationToken);
        if (existing is not null) return existing;

        var created = RfidAccessSession.Create(companyPublicId, command.SessionId, command.UserPublicId, command.WarehousePublicId, command.AccessCardId, command.EdgeDeviceId, command.StartedAt);
        await _sessionRepository.AddAsync(created, cancellationToken);
        return created;
    }

    public async Task<RfidAccessSession?> EndAccessSessionAsync(Guid companyPublicId, EndAccessSessionCommand command, CancellationToken cancellationToken)
    {
        var existing = await _sessionRepository.GetBySessionIdAsync(companyPublicId, command.SessionId, cancellationToken);
        existing?.Close(command.EndedAt);
        return existing;
    }

    public async Task<IngestPortalExitResult> IngestPortalExitEventAsync(Guid companyPublicId, IngestPortalExitEventCommand command, long? consumptionWarehouseId, CancellationToken cancellationToken)
    {
        var existing = await _eventInboxRepository.GetByEventIdAsync(companyPublicId, command.EventId, cancellationToken);
        if (existing is not null)
        {
            return new IngestPortalExitResult(true, 0, [], "Duplicated event");
        }

        var payload = JsonSerializer.Serialize(new { epcs = command.Epcs });
        var inbox = RfidEventInbox.Create(companyPublicId, command.EventId, RfidEventType.PortalExit, command.DeviceCode, command.WarehousePublicId, command.OccurredAt, command.Confidence, payload);
        await _eventInboxRepository.AddAsync(inbox, cancellationToken);

        var activeSession = await _sessionRepository.GetActiveByWarehouseAsync(companyPublicId, command.WarehousePublicId, cancellationToken);
        if (activeSession is null)
        {
            inbox.MarkFailed("No active RFID access session found for warehouse.");
            return new IngestPortalExitResult(false, 0, [], inbox.Error);
        }

        var referenceSessionId = string.IsNullOrWhiteSpace(activeSession.SessionId)
            ? activeSession.PublicId.ToString()
            : activeSession.SessionId;

        var warehouseId = await _referenceResolver.ResolveWarehouseIdAsync(companyPublicId, command.WarehousePublicId, cancellationToken);
        if (!warehouseId.HasValue)
        {
            inbox.MarkFailed("Warehouse not found.");
            return new IngestPortalExitResult(false, 0, [], inbox.Error);
        }

        var company = await _companyRepository.GetByPublicIdAsync(companyPublicId, cancellationToken);
        if (company is null)
        {
            throw new InvalidOperationException("Company not found.");
        }

        var unmapped = new List<string>();
        var created = 0;
        foreach (var epc in command.Epcs.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var tag = await _tagRepository.GetByEpcAsync(companyPublicId, command.WarehousePublicId, epc, cancellationToken);
            if (tag is null || tag.Status != RfidTagStatus.Active)
            {
                unmapped.Add(epc);
                await _movementLinkRepository.AddAsync(RfidMovementLink.Create(companyPublicId, command.EventId, activeSession.SessionId, epc, null, null), cancellationToken);
                continue;
            }

            var productPublicId = await _referenceResolver.ResolveProductPublicIdByEpcAsync(
                companyPublicId,
                command.WarehousePublicId,
                epc,
                cancellationToken);
            if (!productPublicId.HasValue)
            {
                unmapped.Add(epc);
                await _movementLinkRepository.AddAsync(RfidMovementLink.Create(companyPublicId, command.EventId, activeSession.SessionId, epc, null, null), cancellationToken);
                continue;
            }

            var productId = await _referenceResolver.ResolveProductIdAsync(companyPublicId, productPublicId.Value, cancellationToken);
            if (!productId.HasValue)
            {
                unmapped.Add(epc);
                await _movementLinkRepository.AddAsync(RfidMovementLink.Create(companyPublicId, command.EventId, activeSession.SessionId, epc, productPublicId.Value, null), cancellationToken);
                continue;
            }

            var movement = InventoryMovement.Create(
                company.Id,
                productId.Value,
                MovementType.Out,
                1m,
                warehouseId.Value,
                consumptionWarehouseId,
                ReferenceTypeSession,
                referenceSessionId,
                $"{PortalExitReason}. EventId={command.EventId}",
                RfidSource,
                command.DeviceCode);

            await _inventoryMovementRepository.AddAsync(movement, cancellationToken);
            await _movementLinkRepository.AddAsync(RfidMovementLink.Create(companyPublicId, command.EventId, activeSession.SessionId, epc, productPublicId.Value, movement.PublicId), cancellationToken);
            created++;
        }

        if (unmapped.Count > 0)
        {
            inbox.MarkProcessed($"Unmapped EPCs: {string.Join(',', unmapped)}");
        }
        else
        {
            inbox.MarkProcessed();
        }

        return new IngestPortalExitResult(false, created, unmapped, inbox.Error);
    }
}
