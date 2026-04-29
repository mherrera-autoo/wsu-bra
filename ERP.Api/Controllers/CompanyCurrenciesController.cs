using System;
using ERP.Api.Authorization;
using ERP.Api.Contracts.Companies;
using ERP.Modules.MasterData.Application.Services;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/companies/currencies")]
public sealed class CompanyCurrenciesController : ControllerBase
{
    private readonly CompanyCurrencyService _companyCurrencyService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public CompanyCurrenciesController(
        CompanyCurrencyService companyCurrencyService,
        ICurrentUserProvider currentUserProvider)
    {
        _companyCurrencyService = companyCurrencyService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CompanyCurrencySummary>>> ListCurrencies(
        [FromQuery] bool activeOnly = false,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var currencies = await _companyCurrencyService.ListWithAssignmentAsync(companyId, activeOnly, cancellationToken);
        var response = currencies.Select(ToSummary);
        return Ok(response);
    }

    [HttpGet("{currencyId:long}")]
    public async Task<ActionResult<CompanyCurrencySummary>> GetCurrency(long currencyId, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var companyCurrency = await _companyCurrencyService.GetAsync(companyId, currencyId, cancellationToken);
        if (companyCurrency is null)
        {
            return NotFound();
        }

        return Ok(ToSummary(companyCurrency));
    }

	[HttpPost]
    public async Task<IActionResult> CreateCurrency(CreateCompanyCurrencyRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _companyCurrencyService.CreateAsync(
            companyId,
            request.CurrencyId,
            request.IsDefault,
            request.IsActive,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(nameof(GetCurrency), new { currencyId = result.Value!.CurrencyId }, ToSummary(result.Value!));
    }

    [HttpPut("{currencyId:long}")]
    public async Task<IActionResult> UpdateCurrency(
        long currencyId,
        UpdateCompanyCurrencyRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var result = await _companyCurrencyService.UpdateAsync(
            companyId,
            currencyId,
            request.IsDefault,
            request.IsActive,
            cancellationToken);

        if (!result.Success)
        {
            if (string.Equals(result.Error, "Company currency not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(ToSummary(result.Value!));
    }

    [HttpDelete("{currencyId:long}")]
    public async Task<IActionResult> DeleteCurrency(long currencyId, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _companyCurrencyService.DeleteAsync(companyId, currencyId, cancellationToken);
        if (!result.Success)
        {
            if (string.Equals(result.Error, "Company currency not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "deleted" });
    }

    private bool TryGetCompanyId(out long companyId)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null)
        {
            companyId = 0;
            return false;
        }

        companyId = currentUser.CompanyId;
        return true;
    }

    private static CompanyCurrencySummary ToSummary(ERP.Modules.MasterData.Domain.CompanyCurrency companyCurrency)
        => new(
            companyCurrency.CurrencyId,
            companyCurrency.Currency.Code,
            companyCurrency.Currency.Name,
            companyCurrency.Currency.Order,
            companyCurrency.Currency.Symbol,
            IsAssigned: true,
            companyCurrency.IsDefault,
            companyCurrency.IsActive);

    private static CompanyCurrencySummary ToSummary(CompanyCurrencyCatalogItem companyCurrency)
        => new(
            companyCurrency.CurrencyId,
            companyCurrency.Code,
            companyCurrency.Name,
            companyCurrency.Order,
            companyCurrency.Symbol,
            companyCurrency.IsAssigned,
            companyCurrency.IsDefault,
            companyCurrency.IsActive);
}
