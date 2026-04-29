using ERP.Modules.Sales.Domain;

namespace ERP.Modules.Sales.Application.Repositories;

public interface ISalesQuoteRepository
{
    Task AddAsync(SalesQuote quote, CancellationToken cancellationToken = default);
    Task<SalesQuote?> GetByPublicIdAsync(long companyId, Guid publicId, CancellationToken cancellationToken = default);
    Task<string> GetNextQuoteNumberAsync(long companyId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<SalesQuote> Items, int TotalCount)> SearchAsync(
        long companyId,
        SalesQuoteStatus? status,
        string? search,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
