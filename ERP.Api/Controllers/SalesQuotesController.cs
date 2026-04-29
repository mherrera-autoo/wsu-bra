using ERP.Api.Authorization;
using ERP.Modules.Sales.Application.Commands;
using ERP.Modules.Sales.Application.Handlers;
using ERP.Modules.Sales.Contracts;
using ERP.Modules.Sales.Domain;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractSalesQuoteStatus = ERP.Modules.Sales.Contracts.SalesQuoteStatus;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/sales/quotes")]
public sealed class SalesQuotesController : ControllerBase
{
    private readonly CreateSalesQuoteHandler _createHandler;
    private readonly SubmitSalesQuoteHandler _submitHandler;
    private readonly ApproveSalesQuoteHandler _approveHandler;
    private readonly RejectSalesQuoteHandler _rejectHandler;
    private readonly SalesQuoteQueryHandler _queryHandler;
    private readonly ITenantContext _tenantContext;

    public SalesQuotesController(
        CreateSalesQuoteHandler createHandler,
        SubmitSalesQuoteHandler submitHandler,
        ApproveSalesQuoteHandler approveHandler,
        RejectSalesQuoteHandler rejectHandler,
        SalesQuoteQueryHandler queryHandler,
        ITenantContext tenantContext)
    {
        _createHandler = createHandler;
        _submitHandler = submitHandler;
        _approveHandler = approveHandler;
        _rejectHandler = rejectHandler;
        _queryHandler = queryHandler;
        _tenantContext = tenantContext;
    }

    [HttpPost]
    [RequireCompanyPermission(PermissionKeys.Sales.QuotesWrite)]
    public async Task<IActionResult> Create(CreateSalesQuoteApiRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId())
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            return UnprocessableEntity(new { error = "Customer name is required." });
        }

        if (request.Lines is null || request.Lines.Count == 0)
        {
            return UnprocessableEntity(new { error = "At least one line is required." });
        }

        if (request.Lines.Any(line => string.IsNullOrWhiteSpace(line.Description)))
        {
            return UnprocessableEntity(new { error = "Line description is required." });
        }

        if (request.Lines.Any(line => line.Quantity <= 0))
        {
            return UnprocessableEntity(new { error = "Line quantity must be greater than zero." });
        }

        if (request.Lines.Any(line => line.UnitPrice < 0))
        {
            return UnprocessableEntity(new { error = "Line unit price must be >= 0." });
        }

        if (request.Lines.Any(line => line.TaxRate is < 0))
        {
            return UnprocessableEntity(new { error = "Line tax rate must be >= 0." });
        }

        var command = new CreateSalesQuoteCommand(
            request.CustomerName,
            request.Notes,
            request.CurrencyCode,
            request.Lines.Select(line => new CreateSalesQuoteLineCommand(
                line.ProductId,
                line.Description,
                line.Quantity,
                line.UnitPrice,
                line.TaxRate)).ToList());

        var result = await _createHandler.HandleAsync(command, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(MapDetail(result.Value!));
    }

    [HttpGet("{id:guid}")]
    [RequireCompanyPermission(PermissionKeys.Sales.QuotesRead)]
    public async Task<ActionResult<SalesQuoteDetail>> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId())
        {
            return Unauthorized();
        }

        var detail = await _queryHandler.HandleAsync(id, cancellationToken);
        if (detail is null)
        {
            return NotFound();
        }

        return Ok(detail);
    }

    [HttpGet]
    [RequireCompanyPermission(PermissionKeys.Sales.QuotesRead)]
    public async Task<ActionResult<PagedResult<SalesQuoteListItem>>> Search(
        [FromQuery] string? status,
        [FromQuery(Name = "q")] string? query,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetCompanyId())
        {
            return Unauthorized();
        }

        if (page <= 0)
        {
            return UnprocessableEntity(new { error = "Page must be >= 1." });
        }

        if (pageSize <= 0 || pageSize > 200)
        {
            return UnprocessableEntity(new { error = "PageSize must be between 1 and 200." });
        }

        ContractSalesQuoteStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse(status, true, out ContractSalesQuoteStatus parsed))
            {
                return UnprocessableEntity(new { error = "Invalid status filter." });
            }

            parsedStatus = parsed;
        }

        var searchQuery = new SearchSalesQuotesQuery(parsedStatus, query, from, to, page, pageSize);
        var result = await _queryHandler.HandleAsync(searchQuery, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/submit")]
    [RequireCompanyPermission(PermissionKeys.Sales.QuotesWrite)]
    public async Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId())
        {
            return Unauthorized();
        }

        var result = await _submitHandler.HandleAsync(new SubmitSalesQuoteCommand(id), cancellationToken);
        return BuildStateChangeResponse(result);
    }

    [HttpPost("{id:guid}/approve")]
    [RequireCompanyPermission(PermissionKeys.Sales.QuotesApprove)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId())
        {
            return Unauthorized();
        }

        var result = await _approveHandler.HandleAsync(new ApproveSalesQuoteCommand(id), cancellationToken);
        return BuildStateChangeResponse(result);
    }

    [HttpPost("{id:guid}/reject")]
    [RequireCompanyPermission(PermissionKeys.Sales.QuotesApprove)]
    public async Task<IActionResult> Reject(Guid id, RejectSalesQuoteRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId())
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return UnprocessableEntity(new { error = "Reject reason is required." });
        }

        var result = await _rejectHandler.HandleAsync(new RejectSalesQuoteCommand(id, request.Reason), cancellationToken);
        return BuildStateChangeResponse(result);
    }

    private IActionResult BuildStateChangeResponse(Result<SalesQuote> result)
    {
        if (!result.Success)
        {
            if (string.Equals(result.Error, "Sales quote not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            if (string.Equals(result.Error, "Sales quote status conflict.", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new { error = result.Error });
            }

            if (string.Equals(result.Error, "Reject reason is required.", StringComparison.OrdinalIgnoreCase))
            {
                return UnprocessableEntity(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(MapDetail(result.Value!));
    }

    private static SalesQuoteDetail MapDetail(SalesQuote quote)
    {
        var lines = quote.Lines
            .OrderBy(line => line.LineNumber)
            .Select(line => new SalesQuoteLineDetail(
                line.LineNumber,
                line.ProductId,
                line.Description,
                line.Quantity,
                line.UnitPrice,
                line.LineTotal,
                line.TaxRate))
            .ToList();

        return new SalesQuoteDetail(
            quote.PublicId,
            quote.QuoteNumber,
            (ContractSalesQuoteStatus)quote.Status,
            quote.CustomerName,
            quote.Notes,
            quote.CurrencyCode,
            quote.Subtotal,
            quote.TaxTotal,
            quote.Total,
            lines,
            quote.CreatedAt,
            quote.CreatedByUserId,
            quote.UpdatedAt,
            quote.UpdatedByUserId);
    }

    private bool TryGetCompanyId()
        => (_tenantContext.CompanyId ?? 0) > 0;
}
