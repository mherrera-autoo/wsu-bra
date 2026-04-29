using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.Application.Repositories;

public interface ICityRepository
{
    Task AddAsync(City city, CancellationToken cancellationToken = default);
    Task<City?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<City?> GetByOfficialCodeAsync(long countryId, string officialCode, CancellationToken cancellationToken = default);
    Task<City?> GetByNameAsync(long countryId, long? subdivisionId, string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<City>> ListAsync(
        long countryId,
        long? subdivisionId,
        string? search,
        bool activeOnly,
        CancellationToken cancellationToken = default);
}
