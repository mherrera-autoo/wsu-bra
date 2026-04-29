using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.Application.Repositories;

public interface IProductUnitConversionRepository
{
    Task AddAsync(ProductUnitConversion conversion, CancellationToken cancellationToken = default);
    Task<ProductUnitConversion?> GetAsync(long productId, long fromUnitOfMeasureId, long toUnitOfMeasureId, CancellationToken cancellationToken = default);
}
