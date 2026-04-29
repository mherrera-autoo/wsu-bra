using ERP.Modules.Purchasing.Contracts;
using ERP.Modules.Purchasing.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class GoodsReceiptQueryRepository : IGoodsReceiptQuery
{
    private readonly ErpDbContext _dbContext;

    public GoodsReceiptQueryRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GoodsReceiptSnapshot?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default)
    {
        var receipt = await _dbContext.GoodsReceipts
            .AsNoTracking()
            .Include(gr => gr.Lines)
            .FirstOrDefaultAsync(gr => gr.CompanyId == companyId && gr.Id == id, cancellationToken);

        return receipt is null ? null : Map(receipt);
    }

    public async Task<IReadOnlyList<GoodsReceiptSnapshot>> ListAsync(long companyId, CancellationToken cancellationToken = default)
    {
        var receipts = await _dbContext.GoodsReceipts
            .AsNoTracking()
            .Include(gr => gr.Lines)
            .Where(gr => gr.CompanyId == companyId)
            .OrderByDescending(gr => gr.ReceivedAt)
            .ToListAsync(cancellationToken);

        return receipts.Select(Map).ToList();
    }

    private static GoodsReceiptSnapshot Map(GoodsReceipt receipt)
        => new(
            receipt.Id,
            receipt.CompanyId,
            receipt.SupplierId,
            receipt.PurchaseOrderId,
            receipt.Lines
                .Select(line => new GoodsReceiptLineSnapshot(
                    line.ProductId,
                    line.ReceivedQty))
                .ToList());
}
