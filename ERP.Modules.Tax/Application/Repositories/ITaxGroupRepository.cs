using ERP.Modules.Tax.Domain;

namespace ERP.Modules.Tax.Application.Repositories;

public interface ITaxGroupRepository
{
    Task AddAsync(TaxGroup taxGroup, CancellationToken cancellationToken = default);
    Task<TaxGroup?> GetByIdWithRulesAsync(long id, CancellationToken cancellationToken = default);
}
