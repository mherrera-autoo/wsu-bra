using ERP.Modules.MasterData.Application.Commands;
using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.MasterData.Application.Handlers;

public sealed class ProductSupplierCommandHandler
{
    private readonly IProductRepository _productRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IProductSupplierRepository _productSupplierRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductSupplierCommandHandler(
        IProductRepository productRepository,
        ISupplierRepository supplierRepository,
        IProductSupplierRepository productSupplierRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _supplierRepository = supplierRepository;
        _productSupplierRepository = productSupplierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductSupplier>> HandleAsync(UpsertProductSupplierCommand command, CancellationToken cancellationToken = default)
    {
        if (command.UnitPurchasePrice < 0)
        {
            return Result<ProductSupplier>.Fail("UnitPurchasePrice cannot be negative.");
        }

        if (command.MinimumPurchaseLot <= 0)
        {
            return Result<ProductSupplier>.Fail("MinimumPurchaseLot must be greater than zero.");
        }

        var product = await _productRepository.GetByIdAsync(command.CompanyId, command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result<ProductSupplier>.Fail("Product not found.");
        }

        var supplier = await _supplierRepository.GetByIdAsync(command.CompanyId, command.SupplierId, cancellationToken);
        if (supplier is null)
        {
            return Result<ProductSupplier>.Fail("Supplier not found.");
        }

        var productSupplier = await _productSupplierRepository.GetByProductAndSupplierAsync(
            command.CompanyId,
            command.ProductId,
            command.SupplierId,
            cancellationToken);

        if (productSupplier is null)
        {
            productSupplier = ProductSupplier.Create(
                command.CompanyId,
                command.ProductId,
                command.SupplierId,
                command.SupplierSku,
                command.UnitPurchasePrice,
                command.MinimumPurchaseLot);

            await _productSupplierRepository.AddAsync(productSupplier, cancellationToken);
        }
        else
        {
            productSupplier.UpdateCommercialTerms(
                command.SupplierSku,
                command.UnitPurchasePrice,
                command.MinimumPurchaseLot);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ProductSupplier>.Ok(productSupplier);
    }

    public async Task<Result> HandleAsync(DeleteProductSupplierCommand command, CancellationToken cancellationToken = default)
    {
        var productSupplier = await _productSupplierRepository.GetByProductAndSupplierAsync(
            command.CompanyId,
            command.ProductId,
            command.SupplierId,
            cancellationToken);

        if (productSupplier is null)
        {
            return Result.Fail("ProductSupplier not found.");
        }

        _productSupplierRepository.Remove(productSupplier);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
