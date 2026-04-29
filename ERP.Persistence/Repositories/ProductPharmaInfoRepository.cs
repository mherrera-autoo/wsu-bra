using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class ProductPharmaInfoRepository : IProductPharmaInfoRepository
{
    private readonly ErpDbContext _dbContext;

    public ProductPharmaInfoRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ProductPharmaInfo?> GetByProductAsync(long companyId, long productId, CancellationToken cancellationToken = default)
        => _dbContext.ProductPharmaInfos.FirstOrDefaultAsync(info => info.CompanyId == companyId && info.ProductId == productId, cancellationToken);

    public Task<ProductPharmaInfo?> GetByBarcodeAsync(long companyId, string barcode, CancellationToken cancellationToken = default)
    {
        var normalized = barcode.Trim();
        return _dbContext.ProductPharmaInfos.FirstOrDefaultAsync(info =>
            info.CompanyId == companyId && info.Barcode == normalized, cancellationToken);
    }

    public async Task AddAsync(ProductPharmaInfo info, CancellationToken cancellationToken = default)
        => await _dbContext.ProductPharmaInfos.AddAsync(info, cancellationToken);

    public Task UpdateAsync(ProductPharmaInfo info, CancellationToken cancellationToken = default)
    {
        _dbContext.ProductPharmaInfos.Update(info);
        return Task.CompletedTask;
    }
}
