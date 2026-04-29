using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.MasterData.Application.Services;

public interface IUnitOfMeasureConversionService
{
    Task<Result<decimal>> ConvertAsync(
        long productId,
        long fromUnitOfMeasureId,
        long toUnitOfMeasureId,
        decimal quantity,
        CancellationToken cancellationToken = default);
}

public sealed class UnitOfMeasureConversionService : IUnitOfMeasureConversionService
{
    private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;
    private readonly IProductUnitConversionRepository _productUnitConversionRepository;

    public UnitOfMeasureConversionService(
        IUnitOfMeasureRepository unitOfMeasureRepository,
        IProductUnitConversionRepository productUnitConversionRepository)
    {
        _unitOfMeasureRepository = unitOfMeasureRepository;
        _productUnitConversionRepository = productUnitConversionRepository;
    }

    public async Task<Result<decimal>> ConvertAsync(
        long productId,
        long fromUnitOfMeasureId,
        long toUnitOfMeasureId,
        decimal quantity,
        CancellationToken cancellationToken = default)
    {
        if (fromUnitOfMeasureId == toUnitOfMeasureId)
        {
            return Result<decimal>.Ok(quantity);
        }

        var fromUnit = await _unitOfMeasureRepository.GetByIdAsync(fromUnitOfMeasureId, cancellationToken);
        var toUnit = await _unitOfMeasureRepository.GetByIdAsync(toUnitOfMeasureId, cancellationToken);
        if (fromUnit is null || toUnit is null)
        {
            return Result<decimal>.Fail("Unit of measure not found.");
        }

        if (fromUnit.Dimension != toUnit.Dimension)
        {
            return Result<decimal>.Fail("Units of measure must share the same dimension.");
        }

        if (fromUnit.Dimension == UnitOfMeasureDimension.Count)
        {
            var conversion = await _productUnitConversionRepository.GetAsync(
                productId,
                fromUnitOfMeasureId,
                toUnitOfMeasureId,
                cancellationToken);
            if (conversion is null)
            {
                return Result<decimal>.Fail("Product-specific conversion is required for count-based units.");
            }

            return Result<decimal>.Ok(quantity * conversion.Factor);
        }

        if (fromUnit.FactorToBase <= 0 || toUnit.FactorToBase <= 0)
        {
            return Result<decimal>.Fail("Unit of measure factors must be configured.");
        }

        var factor = fromUnit.FactorToBase / toUnit.FactorToBase;
        return Result<decimal>.Ok(quantity * factor);
    }
}
