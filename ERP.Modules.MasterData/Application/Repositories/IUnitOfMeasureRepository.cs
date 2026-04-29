using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.Application.Repositories;

public interface IUnitOfMeasureRepository
{
    Task AddAsync(UnitOfMeasure unitOfMeasure, CancellationToken cancellationToken = default);
    Task<UnitOfMeasure?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<UnitOfMeasure?> GetByCanonicalCodeAsync(string canonicalCode, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCanonicalCodeAsync(string canonicalCode, long? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UnitOfMeasure>> ListAsync(CancellationToken cancellationToken = default);
}
