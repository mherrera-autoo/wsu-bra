using ERP.Api.Authorization;
using ERP.Api.Contracts.Pharmacy;
using ERP.Api.Filters;
using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Services;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceDispenseBatchInput = ERP.Modules.PharmaceuticalRegulatedInventory.Application.Services.DispenseBatchInput;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[RequireFeature(PharmacyFeatureKeys.Dispensing)]
[Route("api/pharmacy/dispenses")]
// TODO: Move dispensing endpoints to ERP.Modules.RetailPharmacy (dispensación/POS/recetas).
public sealed class PharmacyDispensingController : ControllerBase
{
    private readonly PharmacyDispensingService _dispensingService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public PharmacyDispensingController(PharmacyDispensingService dispensingService, ICurrentUserProvider currentUserProvider)
    {
        _dispensingService = dispensingService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpPost]
    public async Task<IActionResult> CreateDispense(CreateDispenseRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var lines = request.Lines.Select(line => (line.ProductId, line.Quantity));
        var prescription = request.Prescription is null
            ? null
            : new PrescriptionInput(
                request.Prescription.Type,
                request.Prescription.IssuedAt,
                request.Prescription.ExpiresAt,
                request.Prescription.Patient,
                request.Prescription.PatientIdentification,
                request.Prescription.Doctor,
                request.Prescription.DoctorIdentification,
                request.Prescription.Folio,
                request.Prescription.AttachmentUrl);

        var result = await _dispensingService.CreateDispenseAsync(
            companyId,
            request.Date,
            request.PerformedByUserId,
            lines,
            prescription,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpPost("{dispenseId:long}/confirm")]
    public async Task<IActionResult> ConfirmDispense(long dispenseId, ConfirmDispenseRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var batchInputs = request.BatchInputs?
            .Select(input => new ServiceDispenseBatchInput(input.ProductId, input.BatchNumber, input.ExpiryDate, input.Quantity))
            .ToList() ?? new List<ServiceDispenseBatchInput>();

        var result = await _dispensingService.ConfirmDispenseAsync(
            companyId,
            dispenseId,
            request.WarehouseId,
            batchInputs,
            cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "ok" });
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
