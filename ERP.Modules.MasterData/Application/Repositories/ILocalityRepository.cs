using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.Application.Repositories;

public interface ILocalityRepository
{
    Task AddAsync(Locality locality, CancellationToken cancellationToken = default);
    Task<Locality?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Locality?> GetByNameAsync(long cityId, string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Locality>> ListAsync(long cityId, bool activeOnly, CancellationToken cancellationToken = default);
}
