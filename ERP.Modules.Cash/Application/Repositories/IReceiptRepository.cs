using ERP.Modules.Cash.Domain;

namespace ERP.Modules.Cash.Application.Repositories;

public interface IReceiptRepository
{
    Task AddAsync(Receipt receipt, CancellationToken cancellationToken = default);
    Task<Receipt?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
}
