using ERP.Modules.Pricing.Domain;

namespace ERP.Modules.Pricing.Application.Repositories;

public interface IPricingPolicyRepository
{
    Task<PricingPolicy?> GetAsync(long companyId, CancellationToken cancellationToken = default);
    Task AddAsync(PricingPolicy policy, CancellationToken cancellationToken = default);
    Task UpdateAsync(PricingPolicy policy, CancellationToken cancellationToken = default);
}
