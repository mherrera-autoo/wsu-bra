using ERP.Modules.Billing.Domain;

namespace ERP.Modules.Billing.Application.Repositories;

public interface IInvoiceRepository
{
    Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<long?> GetMaxNumberAsync(long companyId, CancellationToken cancellationToken = default);
}
