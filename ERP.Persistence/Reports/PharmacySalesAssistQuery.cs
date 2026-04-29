using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Reporting;
using ERP.Modules.Sales.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Reports;

public sealed class PharmacySalesAssistQuery : IPharmacySalesAssistQuery
{
    private readonly ErpDbContext _dbContext;

    public PharmacySalesAssistQuery(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SalesRecommendationLine>> GetRecentRecommendationsAsync(
        long companyId,
        long customerId,
        DateTime since,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var lineQuery = from document in _dbContext.SalesDocuments.AsNoTracking()
            join line in _dbContext.SalesDocumentLines.AsNoTracking() on document.Id equals line.SalesDocumentId
            join product in _dbContext.Products.AsNoTracking() on line.ProductId equals product.Id
            where document.CompanyId == companyId
                && line.CompanyId == companyId
                && product.CompanyId == companyId
                && document.CustomerId == customerId
                && document.Kind == SalesDocumentKind.Invoice
                && (document.Status == SalesDocumentStatus.Approved || document.Status == SalesDocumentStatus.Sent)
                && document.CreatedAt >= since
            select new
            {
                line.ProductId,
                product.Sku,
                product.Name,
                document.CreatedAt,
                line.Qty
            };

        var grouped = await lineQuery
            .GroupBy(row => new { row.ProductId, row.Sku, row.Name })
            .Select(group => new
            {
                group.Key.ProductId,
                group.Key.Sku,
                group.Key.Name,
                LastPurchasedAt = group.Max(row => row.CreatedAt),
                Quantity = group.Sum(row => row.Qty)
            })
            .OrderByDescending(row => row.LastPurchasedAt)
            .ThenBy(row => row.Name)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return grouped
            .Select(row => new SalesRecommendationLine(
                row.ProductId,
                row.Sku,
                row.Name,
                row.LastPurchasedAt,
                row.Quantity))
            .ToList();
    }
}
