using ERP.Modules.Purchasing.Application.Repositories;
using ERP.Modules.Purchasing.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class GoodsReceiptRepository : IGoodsReceiptRepository
{
    private readonly ErpDbContext _dbContext;

    public GoodsReceiptRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(GoodsReceipt goodsReceipt, CancellationToken cancellationToken = default)
    {
        await _dbContext.GoodsReceipts.AddAsync(goodsReceipt, cancellationToken);
    }

    public Task<GoodsReceipt?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => _dbContext.GoodsReceipts
            .Include(gr => gr.Lines)
            .FirstOrDefaultAsync(gr => gr.CompanyId == companyId && gr.Id == id, cancellationToken);

    public async Task<IReadOnlyList<GoodsReceipt>> ListAsync(long companyId, CancellationToken cancellationToken = default)
        => await _dbContext.GoodsReceipts.AsNoTracking()
            .Where(gr => gr.CompanyId == companyId)
            .OrderByDescending(gr => gr.ReceivedAt)
            .ToListAsync(cancellationToken);
}
