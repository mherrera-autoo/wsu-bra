using ERP.Modules.Subscriptions.Domain;

namespace ERP.Modules.Subscriptions.Application.Repositories;

public interface IPlanRepository
{
    Task<Plan?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Plan?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetFeatureKeysAsync(long planId, CancellationToken cancellationToken = default);
    Task AddAsync(Plan plan, CancellationToken cancellationToken = default);
}
