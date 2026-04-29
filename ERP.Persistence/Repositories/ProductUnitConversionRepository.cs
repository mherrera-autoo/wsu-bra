using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class ProductUnitConversionRepository : IProductUnitConversionRepository
{
    private readonly ErpDbContext _dbContext;

    public ProductUnitConversionRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ProductUnitConversion conversion, CancellationToken cancellationToken = default)
    {
        await _dbContext.ProductUnitConversions.AddAsync(conversion, cancellationToken);
    }

    public Task<ProductUnitConversion?> GetAsync(long productId, long fromUnitOfMeasureId, long toUnitOfMeasureId, CancellationToken cancellationToken = default)
        => _dbContext.ProductUnitConversions.FirstOrDefaultAsync(
            conversion => conversion.ProductId == productId
                && conversion.FromUnitOfMeasureId == fromUnitOfMeasureId
                && conversion.ToUnitOfMeasureId == toUnitOfMeasureId,
            cancellationToken);
}
