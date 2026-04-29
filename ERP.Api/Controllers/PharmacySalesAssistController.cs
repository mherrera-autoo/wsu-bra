using ERP.Api.Authorization;
using ERP.Api.Contracts.Pharmacy;
using ERP.Api.Filters;
using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Reporting;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[RequireFeature(PharmacyFeatureKeys.Base)]
[Route("api/pharmacy/sales")]
// TODO: Move sales assist (POS/dispensación) to ERP.Modules.RetailPharmacy.
public sealed class PharmacySalesAssistController : ControllerBase
{
    private readonly IPharmacySalesAssistQuery _salesAssistQuery;
    private readonly ICurrentUserProvider _currentUserProvider;

    public PharmacySalesAssistController(
        IPharmacySalesAssistQuery salesAssistQuery,
        ICurrentUserProvider currentUserProvider)
    {
        _salesAssistQuery = salesAssistQuery;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet("recommendations")]
    [RequireCompanyPermission(PermissionKeys.Inventory.StockRead)]
    public async Task<IActionResult> GetRecommendations([FromQuery] SalesRecommendationQuery query, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var since = DateTime.UtcNow.AddMonths(-6);
        var limit = query.Limit.GetValueOrDefault(3);
        var recommendations = await _salesAssistQuery.GetRecentRecommendationsAsync(
            companyId,
            query.CustomerId,
            since,
            limit,
            cancellationToken);

        return Ok(recommendations);
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
