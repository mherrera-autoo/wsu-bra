using ERP.Modules.Cash.Domain;

namespace ERP.Modules.Cash.Application.Repositories;

public interface IBankStatementRepository
{
    Task AddAsync(BankStatement statement, CancellationToken cancellationToken = default);
    Task<BankStatement?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
}
