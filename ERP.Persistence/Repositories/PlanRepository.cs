using ERP.Modules.Subscriptions.Application.Repositories;
using ERP.Modules.Subscriptions.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class PlanRepository : IPlanRepository
{
    private readonly ErpDbContext _dbContext;

    public PlanRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Plan?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.Set<Plan>()
            .Include(plan => plan.Features)
            .FirstOrDefaultAsync(plan => plan.Id == id, cancellationToken);

    public Task<Plan?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => _dbContext.Set<Plan>()
            .Include(plan => plan.Features)
            .FirstOrDefaultAsync(plan => plan.Code == code, cancellationToken);

    public async Task<IReadOnlyList<string>> GetFeatureKeysAsync(long planId, CancellationToken cancellationToken = default)
        => await _dbContext.Set<PlanFeature>()
            .AsNoTracking()
            .Where(feature => feature.PlanId == planId)
            .Select(feature => feature.FeatureKey)
            .OrderBy(featureKey => featureKey)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Plan plan, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<Plan>().AddAsync(plan, cancellationToken);
    }
}
