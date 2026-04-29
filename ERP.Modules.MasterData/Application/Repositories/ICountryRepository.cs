using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.Application.Repositories;

public interface ICountryRepository
{
    Task AddAsync(Country country, CancellationToken cancellationToken = default);
    Task<Country?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Country?> GetByIso2Async(string iso2, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Country>> ListAsync(bool activeOnly, CancellationToken cancellationToken = default);
}
