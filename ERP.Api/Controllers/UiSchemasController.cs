using ERP.Api.Authorization;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/ui-schemas")]
public sealed class UiSchemasController : ControllerBase
{
    private readonly IFeatureService _featureService;
    private readonly IPharmaProductProfileRepository _profileRepository;
    private readonly ICurrentUserProvider _currentUserProvider;

    public UiSchemasController(
        IFeatureService featureService,
        IPharmaProductProfileRepository profileRepository,
        ICurrentUserProvider currentUserProvider)
    {
        _featureService = featureService;
        _profileRepository = profileRepository;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet("{screen}")]
    public async Task<IActionResult> GetSchema(
        string screen,
        [FromQuery] long companyId,
        [FromQuery] long? productId,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var currentCompanyId))
        {
            return Unauthorized();
        }

        if (companyId != currentCompanyId)
        {
            return Forbid();
        }

        var featureKeys = await _featureService.GetEnabled(companyId, cancellationToken);

        var baseEnabled = featureKeys.Contains(PharmacyFeatureKeys.Base);
        var dispensingEnabled = featureKeys.Contains(PharmacyFeatureKeys.Dispensing);

        var profile = productId.HasValue
            ? await _profileRepository.GetByProductAsync(companyId, productId.Value, cancellationToken)
            : null;

        var schema = screen switch
        {
            "product-editor" => new
            {
                screen,
                fields = new Dictionary<string, object>
                {
                    ["requiresBatch"] = BuildField(baseEnabled, false),
                    ["requiresExpiry"] = BuildField(baseEnabled, false),
                    ["saleCondition"] = BuildField(baseEnabled, baseEnabled),
                    ["isRegulated"] = BuildField(baseEnabled, false)
                }
            },
            "inventory-receipt-line" => new
            {
                screen,
                fields = new Dictionary<string, object>
                {
                    ["batchNumber"] = BuildField(baseEnabled, baseEnabled && profile?.RequiresBatch == true),
                    ["expiryDate"] = BuildField(baseEnabled, baseEnabled && profile?.RequiresExpiry == true)
                }
            },
            "dispense-form" => new
            {
                screen,
                fields = new Dictionary<string, object>
                {
                    ["prescriptionType"] = BuildField(dispensingEnabled, dispensingEnabled && profile?.SaleCondition == PharmacySaleConditions.PrescriptionRequired),
                    ["prescriptionPatient"] = BuildField(dispensingEnabled, dispensingEnabled && profile?.SaleCondition == PharmacySaleConditions.PrescriptionRequired),
                    ["prescriptionDoctor"] = BuildField(dispensingEnabled, dispensingEnabled && profile?.SaleCondition == PharmacySaleConditions.PrescriptionRequired),
                    ["prescriptionFolio"] = BuildField(dispensingEnabled, dispensingEnabled && profile?.SaleCondition == PharmacySaleConditions.PrescriptionRequired),
                    ["prescriptionAttachmentUrl"] = BuildField(dispensingEnabled, false)
                }
            },
            _ => null
        };

        if (schema is null)
        {
            return NotFound();
        }

        return Ok(schema);
    }

    private static object BuildField(bool visible, bool required)
        => new { visible, required };

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
