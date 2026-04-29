using ERP.Api.Authorization;
using ERP.Api.Contracts.Rfid;
using ERP.Modules.Identity.Application.Services;
using ERP.Modules.Identity.Domain;
using ERP.Modules.Rfid.Application.Commands;
using ERP.Modules.Rfid.Application.Repositories;
using ERP.Modules.Rfid.Application.Services;
using ERP.Modules.Rfid.Domain;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/rfid")]
public sealed class RfidController : ControllerBase
{
    private readonly RfidService _rfidService;
    private readonly IRfidTagRepository _tagRepository;
    private readonly IRfidEventInboxRepository _eventInboxRepository;
    private readonly IRfidAccessSessionRepository _accessSessionRepository;
    private readonly IRfidMovementLinkRepository _movementLinkRepository;
    private readonly IRfidOperatorRepository _operatorRepository;
    private readonly IRfidOperatorCredentialRepository _operatorCredentialRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IRbacService _rbacService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public RfidController(RfidService rfidService, IRfidTagRepository tagRepository, IRfidEventInboxRepository eventInboxRepository, IRfidAccessSessionRepository accessSessionRepository, IRfidMovementLinkRepository movementLinkRepository, IRfidOperatorRepository operatorRepository, IRfidOperatorCredentialRepository operatorCredentialRepository, ICurrentUserProvider currentUserProvider, IRbacService rbacService, IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _rfidService = rfidService;
        _tagRepository = tagRepository;
        _eventInboxRepository = eventInboxRepository;
        _accessSessionRepository = accessSessionRepository;
        _movementLinkRepository = movementLinkRepository;
        _operatorRepository = operatorRepository;
        _operatorCredentialRepository = operatorCredentialRepository;
        _currentUserProvider = currentUserProvider;
        _rbacService = rbacService;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    [HttpGet("tags")]
    public async Task<IActionResult> ListTags([FromQuery] RfidTagStatus? status, [FromQuery] Guid? warehousePublicId, [FromQuery] string? epcPrefix, [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken cancellationToken = default)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null || !currentUser.CompanyPublicId.HasValue) return Unauthorized();

        var companyPublicId = currentUser.CompanyPublicId.Value;
        var items = await _tagRepository.ListAsync(companyPublicId, status, warehousePublicId, epcPrefix, skip, take, cancellationToken);
        var total = await _tagRepository.CountAsync(companyPublicId, status, warehousePublicId, epcPrefix, cancellationToken);

        var response = items
            .Select(x => new RfidTagListItemResponse(
                x.Epc,
                x.WarehousePublicId,
                x.Status,
                x.Notes,
                x.Id,
                x.PublicId,
                x.CreatedAt,
                x.UpdatedAt))
            .ToList();

        return Ok(new PaginatedResult<RfidTagListItemResponse>(response, total));
    }

    [HttpGet("tags/{publicId:guid}")]
    public async Task<IActionResult> GetTag(Guid publicId, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId)) return Unauthorized();
        var item = await _tagRepository.GetByPublicIdAsync(companyId, publicId, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("tags/by-epc/{epc}")]
    public async Task<IActionResult> GetTagByEpc(string epc, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId)) return Unauthorized();
        var item = await _tagRepository.GetByEpcAsync(companyId, warehousePublicId: null, epc, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("access-sessions")]
    public async Task<IActionResult> CreateResolvedAccessSession(CreateResolvedAccessSessionRequest request, CancellationToken cancellationToken)
    {
        if (request.CompanyPublicId == Guid.Empty ||
            request.WarehousePublicId == Guid.Empty ||
            string.IsNullOrWhiteSpace(request.DeviceCode) ||
            request.StartedAt == default ||
            request.Status <= 0 ||
            request.OperatorId <= 0)
        {
            return BadRequest(new { error = "CompanyPublicId, WarehousePublicId, DeviceCode, StartedAt, Status and OperatorId are required." });
        }

        var operatorExists = await _accessSessionRepository.OperatorExistsAsync(request.CompanyPublicId, request.OperatorId, cancellationToken);
        if (!operatorExists)
        {
            return Conflict(new { error = "OperatorId does not exist." });
        }

        var session = RfidAccessSession.CreateResolved(
            request.CompanyPublicId,
            request.WarehousePublicId,
            request.DeviceCode,
            request.StartedAt,
            request.Status,
            request.OperatorId,
            request.SessionId,
            request.EndedAt,
            request.FaceTemplateId,
            request.NfcCardUid);

        await _accessSessionRepository.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(new { session.Id, session.PublicId });
    }

    [HttpGet("access-sessions")]
    public async Task<IActionResult> ListAccessSessions([FromQuery] ListResolvedAccessSessionsQuery query, CancellationToken cancellationToken = default)
    {
        if (query.CompanyPublicId == Guid.Empty)
        {
            return BadRequest(new { error = "companyPublicId is required." });
        }

        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 50 : Math.Min(query.PageSize, 200);
        var skip = (page - 1) * pageSize;

        var items = await _accessSessionRepository.ListResolvedAsync(
            query.CompanyPublicId,
            query.WarehousePublicId,
            query.From,
            query.To,
            query.DeviceCode,
            query.OperatorId,
            query.Status,
            skip,
            pageSize,
            cancellationToken);

        var total = await _accessSessionRepository.CountResolvedAsync(
            query.CompanyPublicId,
            query.WarehousePublicId,
            query.From,
            query.To,
            query.DeviceCode,
            query.OperatorId,
            query.Status,
            cancellationToken);

        var response = items.Select(item => new RfidResolvedAccessSessionResponse(
                item.StartedAt,
                item.EndedAt,
                item.Status,
                item.DeviceCode,
                item.SessionId,
                item.WarehousePublicId,
                item.OperatorId,
                item.OperatorPublicId,
                item.OperatorFullName,
                item.FaceTemplateId,
                item.NfcCardUid))
            .ToList();

        return Ok(new PagedResult<RfidResolvedAccessSessionResponse>(response, total, page, pageSize));
    }

    [HttpGet("operators")]
    public async Task<IActionResult> ListOperators([FromQuery] ListOperatorsQuery query, CancellationToken cancellationToken)
    {
        var companyResolution = await ResolveEffectiveCompanyPublicIdAsync(query.CompanyPublicId, cancellationToken);
        if (!companyResolution.Success)
        {
            return companyResolution.Error!;
        }

        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 50 : Math.Min(query.PageSize, 200);
        var skip = (page - 1) * pageSize;

        var items = await _operatorRepository.ListAsync(companyResolution.CompanyPublicId, query.Search, query.IsActive, skip, pageSize, cancellationToken);
        var total = await _operatorRepository.CountAsync(companyResolution.CompanyPublicId, query.Search, query.IsActive, cancellationToken);

        var response = items.Select(item => new RfidOperatorResponse(
                item.Id,
                item.PublicId,
                item.FullName,
                item.DocumentId,
                item.IsActive,
                item.CreatedAt,
                item.UpdatedAt))
            .ToList();

        return Ok(new PagedResult<RfidOperatorResponse>(response, total, page, pageSize));
    }

    [HttpGet("operators/{operatorId:long}/credentials")]
    public async Task<IActionResult> ListOperatorCredentials(long operatorId, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId)) return Unauthorized();

        var items = await _operatorCredentialRepository.ListActiveByOperatorAsync(companyId, operatorId, cancellationToken);
        var response = items.Select(item => new RfidOperatorCredentialResponse(
                item.Id,
                item.PublicId,
                item.OperatorId,
                item.FaceTemplateId,
                item.NfcCardUid,
                item.IsActive,
                item.CreatedAt,
                item.UpdatedAt))
            .ToList();

        return Ok(response);
    }

    [HttpGet("access-sessions/{publicId:guid}")]
    public async Task<IActionResult> GetAccessSession(Guid publicId, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId)) return Unauthorized();
        var session = await _accessSessionRepository.GetByPublicIdAsync(companyId, publicId, cancellationToken);
        return session is null ? NotFound() : Ok(session);
    }

    [HttpPut("access-sessions/{publicId:guid}")]
    public async Task<IActionResult> UpdateAccessSession(Guid publicId, UpdateAccessSessionRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId)) return Unauthorized();
        var session = await _accessSessionRepository.GetByPublicIdAsync(companyId, publicId, cancellationToken);
        if (session is null) return NotFound();
        session.UpdateMetadata(request.AccessCardId, request.DeviceCode);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(session);
    }

    [HttpPost("access-sessions/{publicId:guid}/close")]
    public async Task<IActionResult> CloseSession(Guid publicId, EndAccessSessionRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId)) return Unauthorized();
        var session = await _accessSessionRepository.GetByPublicIdAsync(companyId, publicId, cancellationToken);
        if (session is null) return NotFound();
        session.Close(request.EndedAt);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(session);
    }

    [HttpDelete("access-sessions/{publicId:guid}")]
    public async Task<IActionResult> DeleteSession(Guid publicId, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId)) return Unauthorized();
        var session = await _accessSessionRepository.GetByPublicIdAsync(companyId, publicId, cancellationToken);
        if (session is null) return NotFound();
        session.MarkAbandoned();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("events")]
    public async Task<IActionResult> IngestEvent(IngestPortalExitEventRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId)) return Unauthorized();
        long? consumptionWarehouseId = _configuration.GetValue<long?>("Rfid:ConsumptionWarehouseId");
        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var ingestionResult = await _rfidService.IngestPortalExitEventAsync(companyId, new IngestPortalExitEventCommand(request.EventId, request.OccurredAt, request.DeviceCode, request.WarehousePublicId, request.Epcs, request.Confidence), consumptionWarehouseId, token);
            return Result<IngestPortalExitResult>.Ok(ingestionResult);
        }, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet("events")]
    public async Task<IActionResult> ListEvents([FromQuery] RfidEventType? type, [FromQuery] RfidInboxProcessingStatus? status, [FromQuery] Guid? warehousePublicId, [FromQuery] string? deviceCode, [FromQuery] DateTimeOffset? occurredFrom, [FromQuery] DateTimeOffset? occurredTo, [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken cancellationToken = default)
    {
        if (!TryGetCompanyId(out var companyId)) return Unauthorized();
        var items = await _eventInboxRepository.ListAsync(companyId, type, status, warehousePublicId, deviceCode, occurredFrom, occurredTo, skip, take, cancellationToken);
        var total = await _eventInboxRepository.CountAsync(companyId, type, status, warehousePublicId, deviceCode, occurredFrom, occurredTo, cancellationToken);
        return Ok(new PaginatedResult<RfidEventInbox>(items, total));
    }

    [HttpGet("events/{publicId:guid}")]
    public async Task<IActionResult> GetEvent(Guid publicId, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId)) return Unauthorized();
        var item = await _eventInboxRepository.GetByPublicIdAsync(companyId, publicId, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPut("events/{publicId:guid}")]
    public async Task<IActionResult> UpdateEvent(Guid publicId, UpdateRfidEventRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId)) return Unauthorized();
        var item = await _eventInboxRepository.GetByPublicIdAsync(companyId, publicId, cancellationToken);
        if (item is null) return NotFound();
        item.UpdateAdmin(request.Status, request.Error, request.PayloadJson);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(item);
    }

    [HttpDelete("events/{publicId:guid}")]
    public async Task<IActionResult> DeleteEvent(Guid publicId, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId)) return Unauthorized();
        var item = await _eventInboxRepository.GetByPublicIdAsync(companyId, publicId, cancellationToken);
        if (item is null) return NotFound();
        item.UpdateAdmin(RfidInboxProcessingStatus.Failed, "Deleted by admin", item.PayloadJson);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("events/{publicId:guid}/process")]
    [HttpPost("events/{publicId:guid}/reprocess")]
    public async Task<IActionResult> ReprocessEvent(Guid publicId, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId)) return Unauthorized();
        var item = await _eventInboxRepository.GetByPublicIdAsync(companyId, publicId, cancellationToken);
        if (item is null) return NotFound();
        item.UpdateAdmin(RfidInboxProcessingStatus.Pending, null, item.PayloadJson);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(item);
    }

    [HttpGet("movement-links")]
    public async Task<IActionResult> ListMovementLinks([FromQuery] string? eventId, [FromQuery] string? sessionId, [FromQuery] string? epc, [FromQuery] Guid? productPublicId, [FromQuery] Guid? movementPublicId, [FromQuery] DateTimeOffset? createdFrom, [FromQuery] DateTimeOffset? createdTo, [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken cancellationToken = default)
    {
        if (!TryGetCompanyId(out var companyId)) return Unauthorized();
        var items = await _movementLinkRepository.ListAsync(companyId, eventId, sessionId, epc, productPublicId, movementPublicId, createdFrom, createdTo, skip, take, cancellationToken);
        var total = await _movementLinkRepository.CountAsync(companyId, eventId, sessionId, epc, productPublicId, movementPublicId, createdFrom, createdTo, cancellationToken);
        return Ok(new PaginatedResult<RfidMovementLink>(items, total));
    }

    [HttpGet("movement-links/{publicId:guid}")]
    public async Task<IActionResult> GetMovementLink(Guid publicId, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId)) return Unauthorized();
        var item = await _movementLinkRepository.GetByPublicIdAsync(companyId, publicId, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpDelete("movement-links/{publicId:guid}")]
    public async Task<IActionResult> DeleteMovementLink(Guid publicId, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId)) return Unauthorized();
        var item = await _movementLinkRepository.GetByPublicIdAsync(companyId, publicId, cancellationToken);
        if (item is null) return NotFound();
        _movementLinkRepository.Remove(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task<(bool Success, Guid CompanyPublicId, IActionResult? Error)> ResolveEffectiveCompanyPublicIdAsync(
        Guid? requestedCompanyPublicId,
        CancellationToken cancellationToken)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null || currentUser.UserId <= 0)
        {
            return (false, default, Unauthorized());
        }

        if (requestedCompanyPublicId.HasValue && requestedCompanyPublicId.Value == Guid.Empty)
        {
            return (false, default, BadRequest(new { error = "companyPublicId must be a valid guid." }));
        }

        if (currentUser.CompanyPublicId is not Guid tokenCompanyPublicId || tokenCompanyPublicId == Guid.Empty)
        {
            if (!requestedCompanyPublicId.HasValue)
            {
                return (false, default, Unauthorized());
            }

            var hasPlatformWsuManagePermissionWithoutTenant = await _rbacService.HasPermissionAsync(
                currentUser.UserId,
                PermissionKeys.Platform.WsuManage,
                RoleAssignmentScopeType.Platform,
                organizationId: null,
                companyPublicId: null,
                ct: cancellationToken);
            if (!hasPlatformWsuManagePermissionWithoutTenant)
            {
                return (false, default, Forbid());
            }

            return (true, requestedCompanyPublicId.Value, null);
        }

        if (!requestedCompanyPublicId.HasValue || requestedCompanyPublicId.Value == tokenCompanyPublicId)
        {
            return (true, tokenCompanyPublicId, null);
        }

        var hasPlatformWsuManagePermission = await _rbacService.HasPermissionAsync(
            currentUser.UserId,
            PermissionKeys.Platform.WsuManage,
            RoleAssignmentScopeType.Platform,
            organizationId: null,
            companyPublicId: null,
            ct: cancellationToken);
        if (!hasPlatformWsuManagePermission)
        {
            return (false, default, Forbid());
        }

        return (true, requestedCompanyPublicId.Value, null);
    }

    private bool TryGetCompanyId(out Guid companyId)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null || !currentUser.CompanyPublicId.HasValue)
        {
            companyId = default;
            return false;
        }

        companyId = currentUser.CompanyPublicId.Value;
        return true;
    }
}
