using ERP.Modules.Wms.Domain;

namespace ERP.Modules.Wms.Application.Repositories;

public interface IDispatchConfirmationRepository
{
    Task AddAsync(DispatchConfirmation confirmation, CancellationToken cancellationToken = default);
    Task<DispatchConfirmation?> GetByReferenceAsync(
        long companyId,
        string referenceType,
        string referenceId,
        CancellationToken cancellationToken = default);
}
