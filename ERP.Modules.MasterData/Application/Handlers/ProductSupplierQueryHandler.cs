using ERP.Modules.MasterData.Application.Queries;
using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.Application.Handlers;

public sealed class ProductSupplierQueryHandler
{
    private readonly IProductSupplierRepository _productSupplierRepository;

    public ProductSupplierQueryHandler(IProductSupplierRepository productSupplierRepository)
    {
        _productSupplierRepository = productSupplierRepository;
    }

    public Task<ProductSupplier?> HandleAsync(GetProductSupplierByIdQuery query, CancellationToken cancellationToken = default)
        => _productSupplierRepository.GetByIdAsync(query.CompanyId, query.ProductSupplierId, cancellationToken);

    public Task<ProductSupplier?> HandleAsync(GetProductSupplierByPairQuery query, CancellationToken cancellationToken = default)
        => _productSupplierRepository.GetByProductAndSupplierAsync(query.CompanyId, query.ProductId, query.SupplierId, cancellationToken);

    public Task<IReadOnlyList<ProductSupplier>> HandleAsync(ListProductSuppliersByProductQuery query, CancellationToken cancellationToken = default)
        => _productSupplierRepository.ListByProductAsync(query.CompanyId, query.ProductId, cancellationToken);

    public Task<IReadOnlyList<ProductSupplier>> HandleAsync(ListProductSuppliersBySupplierQuery query, CancellationToken cancellationToken = default)
        => _productSupplierRepository.ListBySupplierAsync(query.CompanyId, query.SupplierId, cancellationToken);
}
