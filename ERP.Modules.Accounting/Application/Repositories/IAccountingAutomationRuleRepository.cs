using ERP.Modules.Accounting.Domain;

namespace ERP.Modules.Accounting.Application.Repositories;

public interface IAccountingAutomationRuleRepository
{
    Task AddAsync(AccountingAutomationRule rule, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccountingAutomationRule>> ListByCompanyAsync(long companyId, CancellationToken cancellationToken = default);
    Task<AccountingAutomationRule?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default);
}
