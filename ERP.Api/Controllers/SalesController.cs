using ERP.Api.Authorization;
using ERP.Api.Contracts.Sales;
using ERP.Modules.Sales.Application.Services;
using ERP.Shared.Application;
using ERP.Shared.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/sales")]
public sealed class SalesController : ControllerBase
{
    private readonly SalesService _salesService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public SalesController(SalesService salesService, ICurrentUserProvider currentUserProvider)
    {
        _salesService = salesService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpPost("documents/quotes")]
    [RequireCompanyPermission(PermissionKeys.Documents.Write)]
    public async Task<IActionResult> CreateQuote(CreateSalesQuoteRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var lines = request.Lines.Select(line =>
            (line.ProductId,
                line.Qty,
                new Money(line.UnitPriceAmount, line.UnitPriceCurrency),
                line.TaxGroupId));

        var result = await _salesService.CreateQuoteAsync(
            companyId,
            request.CustomerId,
            lines,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpPost("orders/{id:long}/approve")]
    [RequireCompanyPermission(PermissionKeys.Documents.Write)]
    public async Task<IActionResult> ApproveOrder(long id, ApproveSalesOrderRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var result = await _salesService.ApproveSalesOrderAsync(
            id,
            companyId,
            request.WarehouseId,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id, result.Value!.Status });
    }

    [HttpGet("documents")]
    [RequireCompanyPermission(PermissionKeys.Documents.Read)]
    public async Task<ActionResult<IReadOnlyList<SalesDocumentSummary>>> ListDocuments(CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var documents = await _salesService.ListDocumentsAsync(companyId, cancellationToken);
        var response = documents.Select(document => new SalesDocumentSummary(
            document.Id,
            document.CustomerId,
            document.Kind.ToString(),
            document.Status.ToString(),
            document.CreatedAt));
        return Ok(response);
    }

    [HttpGet("documents/{id:long}")]
    [RequireCompanyPermission(PermissionKeys.Documents.Read)]
    public async Task<ActionResult<SalesDocumentDetail>> GetDocument(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var document = await _salesService.GetDocumentAsync(companyId, id, cancellationToken);
        if (document is null)
        {
            return NotFound();
        }

        var lines = document.Lines.Select(line => new SalesDocumentLineDetail(
            line.ProductId,
            line.Qty,
            line.UnitPrice.Amount,
            line.UnitPrice.Currency));

        return Ok(new SalesDocumentDetail(
            document.Id,
            document.CustomerId,
            document.Kind.ToString(),
            document.Status.ToString(),
            document.CreatedAt,
            lines.ToList()));
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
