using ERP.Api.Authorization;
using ERP.Api.Contracts.Tax;
using ERP.Modules.Tax.Application.Services;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/taxes")]
public sealed class TaxController : ControllerBase
{
    private readonly TaxService _taxService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public TaxController(TaxService taxService, ICurrentUserProvider currentUserProvider)
    {
        _taxService = taxService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTax(CreateTaxRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var result = await _taxService.CreateTaxAsync(
            companyId,
            request.Code,
            request.Name,
            request.Rate,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpPost("groups")]
    public async Task<IActionResult> CreateTaxGroup(CreateTaxGroupRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var result = await _taxService.CreateTaxGroupAsync(
            companyId,
            request.Name,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpPost("groups/{id:long}/rules")]
    public async Task<IActionResult> AddRule(long id, AddTaxRuleRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var result = await _taxService.AddRuleAsync(
            companyId,
            id,
            request.TaxId,
            request.Rate,
            request.Sequence,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
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
