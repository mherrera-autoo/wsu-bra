using ERP.Modules.Sales.Application.Repositories;
using ERP.Modules.Sales.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class SalesQuoteRepository : ISalesQuoteRepository
{
    private readonly ErpDbContext _dbContext;

    public SalesQuoteRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(SalesQuote quote, CancellationToken cancellationToken = default)
    {
        await _dbContext.SalesQuotes.AddAsync(quote, cancellationToken);
    }

    public Task<SalesQuote?> GetByPublicIdAsync(long companyId, Guid publicId, CancellationToken cancellationToken = default)
        => _dbContext.SalesQuotes
            .Include(q => q.Lines)
            .FirstOrDefaultAsync(q => q.CompanyId == companyId && q.PublicId == publicId, cancellationToken);

    public async Task<string> GetNextQuoteNumberAsync(long companyId, CancellationToken cancellationToken = default)
    {
        var lastNumber = await _dbContext.SalesQuotes
            .AsNoTracking()
            .Where(q => q.CompanyId == companyId)
            .OrderByDescending(q => q.Id)
            .Select(q => q.QuoteNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (long.TryParse(lastNumber, out var parsed))
        {
            return (parsed + 1).ToString();
        }

        var count = await _dbContext.SalesQuotes
            .AsNoTracking()
            .CountAsync(q => q.CompanyId == companyId, cancellationToken);
        return (count + 1).ToString();
    }

    public async Task<(IReadOnlyList<SalesQuote> Items, int TotalCount)> SearchAsync(
        long companyId,
        SalesQuoteStatus? status,
        string? search,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SalesQuotes.AsNoTracking()
            .Where(q => q.CompanyId == companyId);

        if (status is not null)
        {
            query = query.Where(q => q.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(q => q.CustomerName.Contains(term) || q.QuoteNumber.Contains(term));
        }

        if (from.HasValue)
        {
            query = query.Where(q => q.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(q => q.CreatedAt <= to.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
