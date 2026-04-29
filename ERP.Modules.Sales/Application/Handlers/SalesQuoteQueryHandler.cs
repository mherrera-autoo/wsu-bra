using ERP.Modules.Sales.Application.Repositories;
using ERP.Modules.Sales.Contracts;
using ERP.Modules.Sales.Domain;
using ERP.Shared.Application;
using ContractSalesQuoteStatus = ERP.Modules.Sales.Contracts.SalesQuoteStatus;
using DomainSalesQuoteStatus = ERP.Modules.Sales.Domain.SalesQuoteStatus;

namespace ERP.Modules.Sales.Application.Handlers;

public sealed class SalesQuoteQueryHandler
{
    private readonly ISalesQuoteRepository _repository;
    private readonly ITenantContext _tenantContext;

    public SalesQuoteQueryHandler(ISalesQuoteRepository repository, ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<SalesQuoteDetail?> HandleAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        var companyId = _tenantContext.CompanyId ?? 0;
        if (companyId <= 0)
        {
            return null;
        }

        var quote = await _repository.GetByPublicIdAsync(companyId, publicId, cancellationToken);
        return quote is null ? null : MapDetail(quote);
    }

    public async Task<PagedResult<SalesQuoteListItem>> HandleAsync(SearchSalesQuotesQuery query, CancellationToken cancellationToken = default)
    {
        var companyId = _tenantContext.CompanyId ?? 0;
        if (companyId <= 0)
        {
            return new PagedResult<SalesQuoteListItem>(Array.Empty<SalesQuoteListItem>(), 0, query.Page, query.PageSize);
        }

        var status = query.Status is null
            ? null
            : (DomainSalesQuoteStatus?)(int)query.Status.Value;
        var (items, totalCount) = await _repository.SearchAsync(
            companyId,
            status,
            query.Query,
            query.From,
            query.To,
            query.Page,
            query.PageSize,
            cancellationToken);

        var listItems = items.Select(quote => new SalesQuoteListItem(
            quote.PublicId,
            quote.QuoteNumber,
            (ContractSalesQuoteStatus)quote.Status,
            quote.CustomerName,
            quote.Total,
            quote.CreatedAt));

        return new PagedResult<SalesQuoteListItem>(listItems.ToList(), totalCount, query.Page, query.PageSize);
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
}
