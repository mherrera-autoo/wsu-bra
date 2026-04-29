using ERP.Modules.Billing.Domain;

namespace ERP.Modules.Billing.Application.Repositories;

public interface IDebitNoteRepository
{
    Task AddAsync(DebitNote debitNote, CancellationToken cancellationToken = default);
    Task<DebitNote?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<long?> GetMaxNumberAsync(long companyId, CancellationToken cancellationToken = default);
}
