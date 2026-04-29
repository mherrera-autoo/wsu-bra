using System.Text.Json;
using ERP.Api.Authorization;
using ERP.Api.Contracts.Logistics;
using ERP.Api.Filters;
using ERP.Modules.Integrations.Contracts;
using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Contracts;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/logistics/deliveries")]
public sealed class LogisticsDeliveriesController : ControllerBase
{
    private readonly IDeliveryConfirmationRepository _deliveryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outboxRepository;
    private readonly ICurrentUserProvider _currentUserProvider;

    public LogisticsDeliveriesController(
        IDeliveryConfirmationRepository deliveryRepository,
        IUnitOfWork unitOfWork,
        IOutboxRepository outboxRepository,
        ICurrentUserProvider currentUserProvider)
    {
        _deliveryRepository = deliveryRepository;
        _unitOfWork = unitOfWork;
        _outboxRepository = outboxRepository;
        _currentUserProvider = currentUserProvider;
    }

    [HttpPost("confirm")]
    [RequireCompanyPermission(PermissionKeys.Inventory.MovementsCreate)]
    public async Task<IActionResult> ConfirmDelivery(DeliveryConfirmationRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var currentUser))
        {
            return Unauthorized();
        }

        if (request.CompanyId != currentUser.CompanyId)
        {
            return Forbid();
        }

        var confirmation = DeliveryConfirmation.Create(
            request.CompanyId,
            request.ReferenceType,
            request.ReferenceId,
            request.DeliveredAt,
            request.Carrier,
            request.ReceivedBy,
            request.Notes,
            currentUser.UserId);

        await _deliveryRepository.AddAsync(confirmation, cancellationToken);

        var deliveryEvent = new PharmaceuticalRegulatedInventoryDeliveryConfirmed(
            confirmation.CompanyId,
            confirmation.Id,
            confirmation.ReferenceType,
            confirmation.ReferenceId,
            confirmation.DeliveredAt,
            confirmation.Carrier,
            confirmation.ReceivedBy,
            confirmation.ConfirmedByUserId,
            DateTime.UtcNow);
        var payload = JsonSerializer.Serialize(deliveryEvent);
        await _outboxRepository.AddAsync(
            OutboxMessage.Create("logistics.delivery.confirmed", payload),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(new { confirmation.Id });
    }

    private bool TryGetCurrentUser(out CurrentUser currentUser)
    {
        var resolved = _currentUserProvider.GetCurrentUser();
        if (resolved is null)
        {
            currentUser = null!;
            return false;
        }

        currentUser = resolved;
        return true;
    }
}
