using ERP.Api.Authorization;
using ERP.Api.Contracts.PharmaceuticalRegulatedInventory;
using ERP.Api.Filters;
using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[RequireFeature(PharmacyFeatureKeys.Base)]
[Route("api/pharmaceutical-regulated-inventory/alerts")]
public sealed class PharmaceuticalRegulatedInventoryAlertsController : ControllerBase
{
    private readonly IExpirationAlertRuleRepository _expirationRuleRepository;
    private readonly IStockoutThresholdRepository _stockoutThresholdRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserProvider _currentUserProvider;

    public PharmaceuticalRegulatedInventoryAlertsController(
        IExpirationAlertRuleRepository expirationRuleRepository,
        IStockoutThresholdRepository stockoutThresholdRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserProvider currentUserProvider)
    {
        _expirationRuleRepository = expirationRuleRepository;
        _stockoutThresholdRepository = stockoutThresholdRepository;
        _unitOfWork = unitOfWork;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet("expirations")]
    [RequireCompanyPermission(PermissionKeys.Inventory.StockRead)]
    public async Task<IActionResult> GetExpirationRule(CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var rule = await _expirationRuleRepository.GetByCompanyAsync(companyId, cancellationToken);
        if (rule is null)
        {
            return Ok(null);
        }

        return Ok(new
        {
            rule.Id,
            rule.DaysBeforeExpiry,
            rule.IsActive
        });
    }

    [HttpPost("expirations")]
    [RequireCompanyPermission(PermissionKeys.Inventory.AdjustmentsCreate)]
    public async Task<IActionResult> UpsertExpirationRule(UpsertExpirationAlertRuleRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var existing = await _expirationRuleRepository.GetByCompanyAsync(companyId, cancellationToken);
        if (existing is null)
        {
            var rule = ExpirationAlertRule.Create(companyId, request.DaysBeforeExpiry, request.IsActive);
            await _expirationRuleRepository.AddAsync(rule, cancellationToken);
        }
        else
        {
            existing.Update(request.DaysBeforeExpiry, request.IsActive);
            await _expirationRuleRepository.UpdateAsync(existing, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(new { status = "ok" });
    }

    [HttpPost("stockouts")]
    [RequireCompanyPermission(PermissionKeys.Inventory.AdjustmentsCreate)]
    public async Task<IActionResult> UpsertStockoutThreshold(UpsertStockoutThresholdRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var existing = await _stockoutThresholdRepository.GetAsync(
            companyId,
            request.ProductId,
            request.WarehouseId,
            cancellationToken);
        if (existing is null)
        {
            var threshold = StockoutThreshold.Create(companyId, request.ProductId, request.WarehouseId, request.ThresholdQuantity);
            await _stockoutThresholdRepository.AddAsync(threshold, cancellationToken);
        }
        else
        {
            existing.Update(request.ThresholdQuantity);
            await _stockoutThresholdRepository.UpdateAsync(existing, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
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
