using ERP.Modules.Accounting.Domain;

namespace ERP.Modules.Accounting.Application.Repositories;

public interface IAccountsReceivableRepository
{
    Task AddAsync(AccountsReceivable receivable, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccountsReceivable>> GetByCompanyAsync(long companyId, CancellationToken cancellationToken = default);
}
