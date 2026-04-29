using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.Application.Repositories;

public interface ISubdivisionRepository
{
    Task AddAsync(Subdivision subdivision, CancellationToken cancellationToken = default);
    Task<Subdivision?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Subdivision?> GetByCodeAsync(long countryId, string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Subdivision>> ListAsync(
        long countryId,
        short? level,
        long? parentSubdivisionId,
        bool activeOnly,
        CancellationToken cancellationToken = default);
}
