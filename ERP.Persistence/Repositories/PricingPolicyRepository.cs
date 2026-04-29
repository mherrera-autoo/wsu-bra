using ERP.Modules.Pricing.Application.Repositories;
using ERP.Modules.Pricing.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class PricingPolicyRepository : IPricingPolicyRepository
{
    private readonly ErpDbContext _dbContext;

    public PricingPolicyRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PricingPolicy?> GetAsync(long companyId, CancellationToken cancellationToken = default)
        => _dbContext.PricingPolicies.FirstOrDefaultAsync(policy => policy.CompanyId == companyId, cancellationToken);

    public async Task AddAsync(PricingPolicy policy, CancellationToken cancellationToken = default)
    {
        await _dbContext.PricingPolicies.AddAsync(policy, cancellationToken);
    }

    public Task UpdateAsync(PricingPolicy policy, CancellationToken cancellationToken = default)
    {
        _dbContext.PricingPolicies.Update(policy);
        return Task.CompletedTask;
    }
}
