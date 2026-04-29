using ERP.Api.Authorization;
using ERP.Api.Contracts.PharmaceuticalRegulatedInventory;
using ERP.Api.Filters;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
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
[Route("api/pharmaceutical-regulated-inventory/warehouse/locations")]
public sealed class PharmaceuticalRegulatedInventoryWarehouseController : ControllerBase
{
    private readonly IWarehouseLocationRepository _locationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserProvider _currentUserProvider;

    public PharmaceuticalRegulatedInventoryWarehouseController(
        IWarehouseLocationRepository locationRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserProvider currentUserProvider)
    {
        _locationRepository = locationRepository;
        _unitOfWork = unitOfWork;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet]
    [RequireCompanyPermission(PermissionKeys.Inventory.StockRead)]
    public async Task<IActionResult> ListLocations([FromQuery] long warehouseId, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var locations = await _locationRepository.ListByWarehouseAsync(companyId, warehouseId, cancellationToken);
        return Ok(locations.Select(location => new
        {
            location.Id,
            location.WarehouseId,
            location.Aisle,
            location.Rack,
            location.Side,
            location.Level,
            location.Slot,
            location.Capacity,
            location.IsPalletSlot,
            location.IsStackable,
            location.Notes
        }));
    }

    [HttpPost]
    [RequireCompanyPermission(PermissionKeys.Inventory.AdjustmentsCreate)]
    public async Task<IActionResult> CreateLocation(CreateWarehouseLocationRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var location = WarehouseLocation.Create(
            request.CompanyId,
            request.WarehouseId,
            request.Aisle,
            request.Rack,
            request.Side,
            request.Level,
            request.Slot,
            request.Capacity,
            request.IsPalletSlot,
            request.IsStackable,
            request.Notes);

        await _locationRepository.AddAsync(location, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(new { location.Id });
    }

    [HttpPut("{locationId:long}")]
    [RequireCompanyPermission(PermissionKeys.Inventory.AdjustmentsCreate)]
    public async Task<IActionResult> UpdateLocation(long locationId, UpdateWarehouseLocationRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var location = await _locationRepository.GetByIdAsync(companyId, locationId, cancellationToken);
        if (location is null)
        {
            return NotFound(new { error = "Location not found." });
        }

        location.Update(
            request.Aisle,
            request.Rack,
            request.Side,
            request.Level,
            request.Slot,
            request.Capacity,
            request.IsPalletSlot,
            request.IsStackable,
            request.Notes);

        await _locationRepository.UpdateAsync(location, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(new { location.Id });
    }

    private bool TryGetCompanyId(out long companyId)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null)
        {
            companyId = default;
            return false;
        }

        companyId = currentUser.CompanyId;
        return true;
    }
}
