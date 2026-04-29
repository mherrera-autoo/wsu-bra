using ERP.Modules.Subscriptions.Application.Repositories;
using ERP.Modules.Subscriptions.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class SubscriptionRepository : ISubscriptionRepository
{
    private readonly ErpDbContext _dbContext;

    public SubscriptionRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Subscription?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.Set<Subscription>()
            .Include(subscription => subscription.CurrentBillingCycle)
            .Include(subscription => subscription.Trial)
            .Include(subscription => subscription.SeatCount)
            .FirstOrDefaultAsync(subscription => subscription.Id == id, cancellationToken);

    public Task<Subscription?> GetActiveByCompanyAsync(long companyId, CancellationToken cancellationToken = default)
        => _dbContext.Set<Subscription>()
            .Include(subscription => subscription.CurrentBillingCycle)
            .Include(subscription => subscription.Trial)
            .Include(subscription => subscription.SeatCount)
            .FirstOrDefaultAsync(
                subscription => subscription.CompanyId == companyId && subscription.Status == SubscriptionStatus.Active,
                cancellationToken);

    public Task<Subscription?> GetLatestByCompanyAsync(long companyId, CancellationToken cancellationToken = default)
        => _dbContext.Set<Subscription>()
            .Include(subscription => subscription.CurrentBillingCycle)
            .Include(subscription => subscription.Trial)
            .Include(subscription => subscription.SeatCount)
            .OrderByDescending(subscription => subscription.StartedAt)
            .FirstOrDefaultAsync(subscription => subscription.CompanyId == companyId, cancellationToken);

    public async Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<Subscription>().AddAsync(subscription, cancellationToken);
    }
}
