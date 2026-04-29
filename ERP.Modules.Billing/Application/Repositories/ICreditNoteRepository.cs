using ERP.Modules.Billing.Domain;

namespace ERP.Modules.Billing.Application.Repositories;

public interface ICreditNoteRepository
{
    Task AddAsync(CreditNote creditNote, CancellationToken cancellationToken = default);
    Task<CreditNote?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<long?> GetMaxNumberAsync(long companyId, CancellationToken cancellationToken = default);
}
