using ERP.Api.Authorization;
using ERP.Api.Contracts.MasterData;
using ERP.Modules.MasterData.Application.Commands;
using ERP.Modules.MasterData.Application.Handlers;
using ERP.Modules.MasterData.Application.Queries;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/masterdata/suppliers")]
public sealed class MasterDataSuppliersController : ControllerBase
{
    private readonly SupplierCommandHandler _commandHandler;
    private readonly SupplierQueryHandler _queryHandler;
    private readonly ICurrentUserProvider _currentUserProvider;

    public MasterDataSuppliersController(
        SupplierCommandHandler commandHandler,
        SupplierQueryHandler queryHandler,
        ICurrentUserProvider currentUserProvider)
    {
        _commandHandler = commandHandler;
        _queryHandler = queryHandler;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SupplierSummary>>> ListSuppliers(
        [FromQuery] string? q,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var suppliers = await _queryHandler.HandleAsync(
            new ListSuppliersQuery(companyId, q, isActive),
            cancellationToken);

        var response = suppliers.Select(ToSummary);
        return Ok(response);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<SupplierSummary>> GetSupplier(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var supplier = await _queryHandler.HandleAsync(
            new GetSupplierByIdQuery(companyId, id),
            cancellationToken);

        if (supplier is null)
        {
            return NotFound();
        }

        return Ok(ToSummary(supplier));
    }

    [HttpPost]
    public async Task<IActionResult> CreateSupplier(CreateSupplierRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var result = await _commandHandler.HandleAsync(
            new CreateSupplierCommand(companyId, request.Name, request.TaxId, request.Country, request.Currency),
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        var supplier = result.Value!;
        return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, ToSummary(supplier));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateSupplier(long id, UpdateSupplierRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var result = await _commandHandler.HandleAsync(
            new UpdateSupplierCommand(companyId, id, request.Name, request.TaxId, request.Country, request.Currency, request.IsActive),
            cancellationToken);

        if (!result.Success)
        {
            if (string.Equals(result.Error, "Supplier not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(ToSummary(result.Value!));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteSupplier(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _commandHandler.HandleAsync(
            new DeleteSupplierCommand(companyId, id),
            cancellationToken);

        if (!result.Success)
        {
            if (string.Equals(result.Error, "Supplier not found.", StringComparison.OrdinalIgnoreCase))
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
            companyId = default;
            return false;
        }

        companyId = currentUser.CompanyId;
        return true;
    }

    private static SupplierSummary ToSummary(ERP.Modules.MasterData.Domain.Supplier supplier)
        => new(supplier.Id, supplier.Name, supplier.TaxId, supplier.Country?.Name, supplier.DefaultCurrency?.Code, supplier.IsActive);
}
