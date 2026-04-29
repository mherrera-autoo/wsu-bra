using ERP.Api.Authorization;
using ERP.Api.Contracts.Wms;
using ERP.Api.Filters;
using ERP.Modules.Wms.Application.Repositories;
using ERP.Modules.Wms.Domain;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[RequireFeature(WmsFeatureKeys.WmsWarehouse)]
[Route("api/wms/dispatch")]
public sealed class WmsDispatchController : ControllerBase
{
    private readonly IDispatchConfirmationRepository _dispatchConfirmationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserProvider _currentUserProvider;

    public WmsDispatchController(
        IDispatchConfirmationRepository dispatchConfirmationRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserProvider currentUserProvider)
    {
        _dispatchConfirmationRepository = dispatchConfirmationRepository;
        _unitOfWork = unitOfWork;
        _currentUserProvider = currentUserProvider;
    }

    [HttpPost("confirm")]
    [RequireCompanyPermission(PermissionKeys.Inventory.MovementsCreate)]
    public async Task<IActionResult> ConfirmDispatch(DispatchConfirmationRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var currentUser))
        {
            return Unauthorized();
        }

        if (request.CompanyId != currentUser.CompanyId)
        {
            return Forbid();
        }

        var existing = await _dispatchConfirmationRepository.GetByReferenceAsync(
            currentUser.CompanyId,
            request.ReferenceType,
            request.ReferenceId,
            cancellationToken);
        if (existing is not null)
        {
            return BadRequest(new { error = "Dispatch already confirmed." });
        }

        var confirmation = DispatchConfirmation.Create(
            currentUser.CompanyId,
            request.ReferenceType,
            request.ReferenceId,
            request.DeliveredAt,
            request.Carrier,
            request.DriverName,
            request.VehiclePlate,
            request.ReceivedBy,
            request.Notes,
            currentUser.UserId);

        await _dispatchConfirmationRepository.AddAsync(confirmation, cancellationToken);
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
