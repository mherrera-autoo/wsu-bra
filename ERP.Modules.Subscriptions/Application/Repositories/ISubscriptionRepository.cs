using ERP.Modules.Subscriptions.Domain;

namespace ERP.Modules.Subscriptions.Application.Repositories;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Subscription?> GetActiveByCompanyAsync(long companyId, CancellationToken cancellationToken = default);
    Task<Subscription?> GetLatestByCompanyAsync(long companyId, CancellationToken cancellationToken = default);
    Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default);
}
