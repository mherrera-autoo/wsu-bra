using ERP.Modules.MasterData.Application.Queries;
using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.Application.Handlers;

public sealed class SupplierQueryHandler
{
    private readonly ISupplierRepository _supplierRepository;

    public SupplierQueryHandler(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<IReadOnlyList<Supplier>> HandleAsync(ListSuppliersQuery query, CancellationToken cancellationToken = default)
    {
        var isActive = query.IsActive ?? true;
        return await _supplierRepository.ListAsync(query.CompanyId, query.Search, isActive, cancellationToken);
    }

    public async Task<Supplier?> HandleAsync(GetSupplierByIdQuery query, CancellationToken cancellationToken = default)
    {
        var supplier = await _supplierRepository.GetByIdAsync(query.CompanyId, query.SupplierId, cancellationToken);
        if (supplier is null || !supplier.IsActive)
        {
            return null;
        }

        return supplier;
    }
}
