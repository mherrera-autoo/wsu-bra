using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class PurchaseInvoiceIngestionRepository : IPurchaseInvoiceIngestionRepository
{
    private readonly ErpDbContext _dbContext;

    public PurchaseInvoiceIngestionRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(PurchaseInvoiceIngestion ingestion, CancellationToken cancellationToken = default)
        => await _dbContext.PurchaseInvoiceIngestions.AddAsync(ingestion, cancellationToken);

    public Task<PurchaseInvoiceIngestion?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => _dbContext.PurchaseInvoiceIngestions
            .Include(ingestion => ingestion.Lines)
            .FirstOrDefaultAsync(ingestion => ingestion.CompanyId == companyId && ingestion.Id == id, cancellationToken);

    public Task<PurchaseInvoiceIngestion?> GetByGoodsReceiptIdAsync(long companyId, long goodsReceiptId, CancellationToken cancellationToken = default)
        => _dbContext.PurchaseInvoiceIngestions
            .Include(ingestion => ingestion.Lines)
            .FirstOrDefaultAsync(ingestion => ingestion.CompanyId == companyId && ingestion.GoodsReceiptId == goodsReceiptId, cancellationToken);

    public Task UpdateAsync(PurchaseInvoiceIngestion ingestion, CancellationToken cancellationToken = default)
    {
        _dbContext.PurchaseInvoiceIngestions.Update(ingestion);
        return Task.CompletedTask;
    }
}
