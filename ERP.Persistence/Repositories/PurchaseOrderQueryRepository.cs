using ERP.Modules.Purchasing.Contracts;
using ERP.Modules.Purchasing.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class PurchaseOrderQueryRepository : IPurchaseOrderQuery
{
    private readonly ErpDbContext _dbContext;

    public PurchaseOrderQueryRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PurchaseOrderSnapshot?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var purchaseOrder = await _dbContext.PurchaseOrders
            .AsNoTracking()
            .Include(po => po.Lines)
            .FirstOrDefaultAsync(po => po.Id == id, cancellationToken);

        return purchaseOrder is null ? null : Map(purchaseOrder);
    }

    public async Task<PurchaseOrderSnapshot?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default)
    {
        var purchaseOrder = await _dbContext.PurchaseOrders
            .AsNoTracking()
            .Include(po => po.Lines)
            .FirstOrDefaultAsync(po => po.CompanyId == companyId && po.Id == id, cancellationToken);

        return purchaseOrder is null ? null : Map(purchaseOrder);
    }

    public async Task<IReadOnlyList<PurchaseOrderSnapshot>> ListAsync(long companyId, CancellationToken cancellationToken = default)
    {
        var purchaseOrders = await _dbContext.PurchaseOrders
            .AsNoTracking()
            .Include(po => po.Lines)
            .Where(po => po.CompanyId == companyId)
            .OrderByDescending(po => po.CreatedAt)
            .ToListAsync(cancellationToken);

        return purchaseOrders.Select(Map).ToList();
    }

    private static PurchaseOrderSnapshot Map(PurchaseOrder purchaseOrder)
        => new(
            purchaseOrder.Id,
            purchaseOrder.CompanyId,
            purchaseOrder.SupplierId,
            purchaseOrder.Currency,
            purchaseOrder.Lines
                .Select(line => new PurchaseOrderLineSnapshot(
                    line.ProductId,
                    line.OrderedQty,
                    line.UnitPriceRef))
                .ToList());
}
