using ERP.Modules.Cash.Domain;

namespace ERP.Modules.Cash.Application.Repositories;

public interface IBankAccountRepository
{
    Task AddAsync(BankAccount bankAccount, CancellationToken cancellationToken = default);
    Task<BankAccount?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
}
