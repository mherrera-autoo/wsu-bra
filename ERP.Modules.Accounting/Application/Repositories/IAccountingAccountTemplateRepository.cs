using ERP.Modules.Accounting.Domain;

namespace ERP.Modules.Accounting.Application.Repositories;

public interface IAccountingAccountTemplateRepository
{
    Task<IReadOnlyList<AccountingAccountTemplate>> ListByVersionAsync(string version, CancellationToken cancellationToken = default);
    Task AddAsync(AccountingAccountTemplate template, CancellationToken cancellationToken = default);
}
