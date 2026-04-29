using ERP.Modules.Accounting.Domain;

namespace ERP.Modules.Accounting.Application.Repositories;

public interface IAccountsPayableRepository
{
    Task AddAsync(AccountsPayable accountsPayable, CancellationToken cancellationToken = default);
    Task<AccountsPayable?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
}
