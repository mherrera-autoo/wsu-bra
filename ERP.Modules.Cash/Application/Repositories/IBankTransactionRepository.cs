using ERP.Modules.Cash.Domain;

namespace ERP.Modules.Cash.Application.Repositories;

public interface IBankTransactionRepository
{
    Task AddAsync(BankTransaction transaction, CancellationToken cancellationToken = default);
    Task<BankTransaction?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
}
