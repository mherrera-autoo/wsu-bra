using ERP.Modules.Purchasing.Application.Repositories;
using ERP.Modules.Purchasing.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly ErpDbContext _dbContext;

    public PurchaseOrderRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default)
    {
        await _dbContext.PurchaseOrders.AddAsync(purchaseOrder, cancellationToken);
    }

    public Task<PurchaseOrder?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.PurchaseOrders
            .Include(po => po.Lines)
            .FirstOrDefaultAsync(po => po.Id == id, cancellationToken);

    public Task<PurchaseOrder?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => _dbContext.PurchaseOrders
            .Include(po => po.Lines)
            .FirstOrDefaultAsync(po => po.CompanyId == companyId && po.Id == id, cancellationToken);

    public async Task<IReadOnlyList<PurchaseOrder>> ListAsync(long companyId, CancellationToken cancellationToken = default)
        => await _dbContext.PurchaseOrders.AsNoTracking()
            .Where(po => po.CompanyId == companyId)
            .OrderByDescending(po => po.CreatedAt)
            .ToListAsync(cancellationToken);
}
