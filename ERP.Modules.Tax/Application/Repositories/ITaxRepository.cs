using TaxEntity = ERP.Modules.Tax.Domain.Tax;

namespace ERP.Modules.Tax.Application.Repositories;

public interface ITaxRepository
{
    Task AddAsync(TaxEntity tax, CancellationToken cancellationToken = default);
    Task<TaxEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
}
