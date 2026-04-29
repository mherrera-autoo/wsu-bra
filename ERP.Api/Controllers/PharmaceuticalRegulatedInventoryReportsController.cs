using ERP.Api.Authorization;
using ERP.Api.Filters;
using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Reporting;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiControlledSubstanceReportQuery = ERP.Api.Contracts.PharmaceuticalRegulatedInventory.ControlledSubstanceReportQuery;
using ApiExpiringStockReportQuery = ERP.Api.Contracts.PharmaceuticalRegulatedInventory.ExpiringStockReportQuery;
using ApiInventoryDifferenceReportQuery = ERP.Api.Contracts.PharmaceuticalRegulatedInventory.InventoryDifferenceReportQuery;
using ApiStockoutReportQuery = ERP.Api.Contracts.PharmaceuticalRegulatedInventory.StockoutReportQuery;
using ApiWasteReportQuery = ERP.Api.Contracts.PharmaceuticalRegulatedInventory.WasteReportQuery;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[RequireFeature(PharmacyFeatureKeys.Base)]
[Route("api/pharmaceutical-regulated-inventory/reports")]
public sealed class PharmaceuticalRegulatedInventoryReportsController : ControllerBase
{
    private readonly IPharmacyReportsQuery _reportsQuery;
    private readonly IExpirationAlertRuleRepository _expirationRuleRepository;
    private readonly ICurrentUserProvider _currentUserProvider;

    public PharmaceuticalRegulatedInventoryReportsController(
        IPharmacyReportsQuery reportsQuery,
        IExpirationAlertRuleRepository expirationRuleRepository,
        ICurrentUserProvider currentUserProvider)
    {
        _reportsQuery = reportsQuery;
        _expirationRuleRepository = expirationRuleRepository;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet("expiring-stock")]
    [RequireCompanyPermission(PermissionKeys.Inventory.StockRead)]
    public async Task<IActionResult> GetExpiringStock([FromQuery] ApiExpiringStockReportQuery query, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var daysThreshold = query.DaysThreshold;
        if (!daysThreshold.HasValue)
        {
            var rule = await _expirationRuleRepository.GetByCompanyAsync(companyId, cancellationToken);
            if (rule is { IsActive: true })
            {
                daysThreshold = rule.DaysBeforeExpiry;
            }
        }

        var reportQuery = new ExpiringStockReportQuery(
            query.From,
            query.To,
            daysThreshold,
            query.ProductId,
            query.WarehouseId,
            query.HealthStatus);
        var result = await _reportsQuery.GetExpiringStockAsync(
            companyId,
            reportQuery,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("controlled-substances")]
    [RequireFeature(PharmacyFeatureKeys.ComplianceIsp)]
    [RequireCompanyPermission(PermissionKeys.Pharmaceutical.ControlledBookRead)]
    public async Task<IActionResult> GetControlledSubstances([FromQuery] ApiControlledSubstanceReportQuery query, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var reportQuery = new ControlledSubstanceReportQuery(
            query.From,
            query.To,
            query.ProductId,
            query.MovementType,
            query.BatchNumber);
        var result = await _reportsQuery.GetControlledSubstancesAsync(
            companyId,
            reportQuery,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("waste")]
    [RequireCompanyPermission(PermissionKeys.Inventory.StockRead)]
    public async Task<IActionResult> GetWaste([FromQuery] ApiWasteReportQuery query, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var reportQuery = new WasteReportQuery(
            query.From,
            query.To,
            query.ProductId,
            query.WarehouseId);
        var result = await _reportsQuery.GetWasteAsync(
            companyId,
            reportQuery,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("inventory-differences")]
    [RequireCompanyPermission(PermissionKeys.Inventory.StockRead)]
    public async Task<IActionResult> GetInventoryDifferences([FromQuery] ApiInventoryDifferenceReportQuery query, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var reportQuery = new InventoryDifferenceReportQuery(
            query.From,
            query.To,
            query.ProductId,
            query.WarehouseId,
            query.ReferenceType);
        var result = await _reportsQuery.GetInventoryDifferencesAsync(
            companyId,
            reportQuery,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("stockouts")]
    [RequireCompanyPermission(PermissionKeys.Inventory.StockRead)]
    public async Task<IActionResult> GetStockouts([FromQuery] ApiStockoutReportQuery query, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var reportQuery = new StockoutReportQuery(
            query.ProductId,
            query.WarehouseId);
        var result = await _reportsQuery.GetStockoutsAsync(
            companyId,
            reportQuery,
            cancellationToken);

        return Ok(result);
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
